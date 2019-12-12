using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Versioning;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.IdentityModel;
using Thinktecture.Relay.OnPremiseConnector.Interceptor;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	// Todo: Move Handler management to separate class
	// Todo: Move Auth to separate class
	internal class RelayServerConnection : IRelayServerConnection
	{
		private const int _CONNECTOR_VERSION = 3;

		private static int _nextInstanceId;
		private static readonly Random _random = new Random();

		private readonly Assembly _versionAssembly;
		private readonly string _userName;
		private readonly string _password;
		private readonly TimeSpan _requestTimeout;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
		private readonly ILogger _logger;
		private readonly IOnPremiseInterceptorFactory _onPremiseInterceptorFactory;
		private readonly ConcurrentDictionary<string, IOnPremiseTargetConnector> _connectors;
		private readonly IRelayServerHttpConnection _httpConnection;
		private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

		private CancellationTokenSource _cts;
		private bool _stopRequested;
		private string _tokenType;
		private string _accessToken;
		private TimeSpan _minReconnectWaitTime = TimeSpan.FromSeconds(2);
		private TimeSpan _maxReconnectWaitTime = TimeSpan.FromSeconds(30);
		private bool _logSensitiveData;
		private ISignalRConnection _signalrClient;

		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler Disposing;

		public Uri Uri { get; }
		public string RelayedRequestHeader { get; set; }
		public TimeSpan TokenRefreshWindow { get; private set; }
		public DateTime TokenExpiry { get; private set; } = DateTime.MaxValue;
		public int RelayServerConnectionInstanceId { get; }
		public DateTime LastHeartbeat { get; private set; } = DateTime.MinValue;
		public TimeSpan HeartbeatInterval { get; private set; }
		public DateTime? ConnectedSince { get; private set; }
		public DateTime? LastActivity { get; private set; }
		public TimeSpan? AbsoluteConnectionLifetime { get; private set; }
		public TimeSpan? SlidingConnectionLifetime { get; private set; }

		public RelayServerConnection(Assembly versionAssembly, string userName, string password, Uri relayServerUri, TimeSpan requestTimeout,
			TimeSpan tokenRefreshWindow, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, IRelayServerHttpConnection httpConnection,
			ILogger logger, bool logSensitiveData, IOnPremiseInterceptorFactory onPremiseInterceptorFactory)
		{
			RelayServerConnectionInstanceId = Interlocked.Increment(ref _nextInstanceId);

			_versionAssembly = versionAssembly ?? throw new ArgumentNullException(nameof(versionAssembly));
			_userName = userName;
			_password = password;
			_requestTimeout = requestTimeout;

			Uri = relayServerUri;
			TokenRefreshWindow = tokenRefreshWindow;

			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
			_httpConnection = httpConnection ?? throw new ArgumentNullException(nameof(httpConnection));
			_logger = logger;
			_logSensitiveData = logSensitiveData;
			_onPremiseInterceptorFactory = onPremiseInterceptorFactory ?? throw new ArgumentNullException(nameof(onPremiseInterceptorFactory));

			_connectors = new ConcurrentDictionary<string, IOnPremiseTargetConnector>(StringComparer.OrdinalIgnoreCase);
			_cts = new CancellationTokenSource();
		}

		#region Targets

		public void RegisterOnPremiseTarget(string key, Uri baseUri, bool followRedirects)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (baseUri == null)
				throw new ArgumentNullException(nameof(baseUri));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise web target. handler-key={HandlerKey}, base-uri={BaseUri}, follow-redirects={FollowRedirects}", key, baseUri, followRedirects);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(baseUri, _requestTimeout, followRedirects, _logSensitiveData);
		}

		public void RegisterOnPremiseTarget(string key, Type handlerType)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, handlerType);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerType, _requestTimeout, _logSensitiveData);
		}

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target using a handler factory. handler-key={HandlerKey}", key);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create(handlerFactory, _requestTimeout, _logSensitiveData);
		}

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new()
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);

			_logger?.Verbose("Registering on-premise in-proc target. handler-key={HandlerKey}, handler-type={HandlerType}", key, typeof(T).Name);

			_connectors[key] = _onPremiseTargetConnectorFactory.Create<T>(_requestTimeout, _logSensitiveData);
		}

		public void RemoveOnPremiseTarget(string key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			key = RemoveTrailingSlashes(key);
			_connectors.TryRemove(key, out var old);
		}

		public List<string> GetOnPremiseTargetKeys()
		{
			return _connectors.Keys.ToList();
		}

		private static string RemoveTrailingSlashes(string key)
		{
			while (key.EndsWith("/"))
			{
				key = key.Substring(0, key.Length - 1);
			}

			return key;
		}

		#endregion

		#region Auth

		public async Task<bool> TryRequestAuthorizationTokenAsync()
		{
			var tokenResponse = await GetAuthorizationTokenAsync().ConfigureAwait(false);

			if (_stopRequested)
			{
				return false;
			}

			CheckResponseTokenForErrors(tokenResponse);
			SetBearerToken(tokenResponse);

			return true;
		}

		private async Task<TokenResponse> GetAuthorizationTokenAsync()
		{
			var client = new OAuth2Client(new Uri(Uri, "/token"));

			while (!_stopRequested)
			{
				try
				{
					_logger?.Verbose("Requesting authorization token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

					var response = await client.RequestResourceOwnerPasswordAsync(_userName, _password).ConfigureAwait(false);
					if (response.IsError)
						throw new AuthenticationException(response.HttpErrorReason ?? response.Error);

					_logger?.Verbose("Received token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
					return response;
				}
				catch (Exception ex)
				{
					var randomWaitTime = GetRandomWaitTime();
					_logger?.Error(ex, "Could not authenticate with RelayServer - re-trying in {RetryWaitTime} seconds. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", randomWaitTime.TotalSeconds, Uri, RelayServerConnectionInstanceId);
					await Task.Delay(randomWaitTime, _cts.Token).ConfigureAwait(false);
				}
			}

			return null;
		}

		private void CheckResponseTokenForErrors(TokenResponse token)
		{
			if (token.IsHttpError)
			{
				_logger?.Warning("Could not authenticate with RelayServer. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, status-code={ConnectionHttpStatusCode}, reason={ConnectionErrorReason}", Uri, RelayServerConnectionInstanceId, token.HttpErrorStatusCode, token.HttpErrorReason);
				throw new AuthenticationException("Could not authenticate with RelayServer: " + token.HttpErrorReason);
			}

			if (token.IsError)
			{
				_logger?.Warning("Could not authenticate with RelayServer. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, reason={ConnectionErrorReason}", Uri, RelayServerConnectionInstanceId, token.Error);
				throw new AuthenticationException("Could not authenticate with RelayServer: " + token.Error);
			}
		}

		private void SetBearerToken(TokenResponse tokenResponse)
		{
			_tokenType = tokenResponse.TokenType;
			_accessToken = tokenResponse.AccessToken;
			TokenExpiry = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

			_httpConnection.SetBearerToken(_accessToken);
			if (_signalrClient != null)
			{
				_signalrClient.AccessToken = new KeyValuePair<string, string>(_tokenType, _accessToken);
			}

			_logger?.Verbose("Setting access token. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, token-expiry={TokenExpiry}", Uri, RelayServerConnectionInstanceId, TokenExpiry);
		}

		#endregion

		#region Connection

		public async Task ConnectAsync()
		{
			_logger?.Information("Connecting to RelayServer {RelayServerUri} with connection instance {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			_stopRequested = false;

			if (!await TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
			{
				return;
			}

			try
			{
				await CreateSignalRClientAsync().ConfigureAwait(false);

				await _signalrClient.Start().ConfigureAwait(false);
				ConnectedSince = DateTime.UtcNow;

				_logger?.Information("Connected to RelayServer {RelayServerUri} with connection {ConnectionId}", Uri, _signalrClient.ConnectionId);

				try
				{
					Connected?.Invoke(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error handling connected event. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error while connecting to RelayServer {RelayServerUri} with connection instance {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
			}
		}

		public void Reconnect()
		{
			_logger?.Debug("Forcing reconnect. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			Disconnect(true);

			Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
		}

		public void Disconnect()
		{
			Disconnect(false);
		}

		private async Task CreateSignalRClientAsync()
		{
			if (_signalrClient != null)
			{
				// Cleanup
				_signalrClient.RequestReceived -= RequestReceived;
				_signalrClient.ConnectionClosed -= ConnectionClosed;
				_signalrClient.Reconnecting -= OnReconnecting;
				_signalrClient.Reconnected -= OnReconnected;

				_signalrClient.Dispose();
				_signalrClient = null;
			}

			var isCoreServer = await DetermineIsNetCoreServerAsync().ConfigureAwait(false);

			_signalrClient = isCoreServer
				? (ISignalRConnection) new Core.SignalRConnection(_logger, Uri, _versionAssembly, _CONNECTOR_VERSION, _tokenType, _accessToken)
				: new Classic.SignalRConnection(_logger, Uri, _versionAssembly, _CONNECTOR_VERSION, _tokenType, _accessToken);

			_signalrClient.RequestReceived += RequestReceived;
			_signalrClient.ConnectionClosed += ConnectionClosed;
			_signalrClient.Reconnecting += OnReconnecting;
			_signalrClient.Reconnected += OnReconnected;
		}

		private async Task<bool> DetermineIsNetCoreServerAsync()
		{
			var response = await GetToRelayAsync("/Version", CancellationToken.None).ConfigureAwait(false);

			if (response.IsSuccessStatusCode)
			{
				using (var sr = new JsonTextReader(new StreamReader(await response.Content.ReadAsStreamAsync())))
				{
					var version = _jsonSerializer.Deserialize<RelayServerVersion>(sr);
					return version.SemanticRelayVersion.Major >= 3;
				}
			}

			return false;
		}

		private class RelayServerVersion
		{
			public string RelayVersion { get; set; }
			public string HostVersion { get; set; }

			[JsonIgnore]
			public SemanticVersion SemanticRelayVersion => SemanticVersion.Parse(RelayVersion);
			[JsonIgnore]
			public SemanticVersion SemanticHostVersion => SemanticVersion.Parse(HostVersion);
		}

		private void Disconnect(bool reconnecting)
		{
			if (reconnecting)
			{
				_logger?.Debug("Forcing reconnect. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
			}

			_logger?.Information("Disconnecting from RelayServer {RelayServerUri} with connection instance {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			LastHeartbeat = DateTime.MinValue;
			HeartbeatInterval = TimeSpan.Zero;

			if (!reconnecting)
			{
				ConnectedSince = null;
				LastActivity = null;
			}

			_stopRequested = true;
			_signalrClient.Stop();
		}

		private void OnReconnected()
		{
			_logger?.Verbose("Connection restored. connection-id={ConnectionId}", _signalrClient.ConnectionId);
		}

		private void OnReconnecting()
		{
			_logger?.Verbose("Connection lost. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
		}

		private void ConnectionClosed()
		{
			_logger?.Information("Connection closed to RelayServer {RelayServerUri} with connection instance {RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);

			if (!_stopRequested)
			{
				var randomWaitTime = GetRandomWaitTime();
				_logger?.Debug("Connection closed. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}, reconnect-wait-time={ReconnectWaitTime}", Uri, RelayServerConnectionInstanceId, randomWaitTime.TotalSeconds);
				Task.Delay(randomWaitTime).ContinueWith(_ => ConnectAsync()).ConfigureAwait(false);
			}

			try
			{
				Disconnected?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error handling disconnected event. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", Uri, RelayServerConnectionInstanceId);
			}
		}

		#endregion

		// Shared usage by Auth and Connection code
		private TimeSpan GetRandomWaitTime()
		{
			return TimeSpan.FromSeconds(_random.Next((int)_minReconnectWaitTime.TotalSeconds, (int)_maxReconnectWaitTime.TotalSeconds));
		}

		public Task SendAcknowledgmentAsync(Guid acknowledgeOriginId, string acknowledgeId, string connectionId = null)
		{
			return GetToRelayAsync($"/request/acknowledge?oid={acknowledgeOriginId}&aid={acknowledgeId}&cid={connectionId ?? _signalrClient.ConnectionId}", CancellationToken.None);
		}

		private async Task AcknowledgeRequestAsync(IOnPremiseTargetRequest request)
		{
			if (request.AcknowledgmentMode == AcknowledgmentMode.Auto)
			{
				_logger?.Debug("Automatically acknowledged by RelayServer. request-id={RequestId}", request.RequestId);
				return;
			}

			if (String.IsNullOrEmpty(request.AcknowledgeId))
			{
				_logger?.Debug("No acknowledgment possible. request-id={RequestId}, acknowledgment-mode={AcknowledgmentMode}", request.RequestId, request.AcknowledgmentMode);
				return;
			}

			switch (request.AcknowledgmentMode)
			{
				case AcknowledgmentMode.Default:
					_logger?.Debug("Sending acknowledge to RelayServer. request-id={RequestId}, origin-id={OriginId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeOriginId, request.AcknowledgeId);
					await SendAcknowledgmentAsync(request.AcknowledgeOriginId, request.AcknowledgeId).ConfigureAwait(false);
					break;

				case AcknowledgmentMode.Manual:
					_logger?.Debug("Manual acknowledgment needed. request-id={RequestId}, origin-id={OriginId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeOriginId, request.AcknowledgeId);
					break;

				default:
					_logger?.Warning("Unknown acknowledgment mode found. request-id={RequestId}, acknowledgment-mode={AcknowledgmentMode}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgmentMode, request.AcknowledgeId);
					break;
			}
		}

		// async void is okay here, as event handlers are the one exception that the guidelines say its okay to use void for
		protected async void RequestReceived(object sender, RequestReceivedEventArgs args)
		{
			var request = args?.Request ?? throw new ArgumentNullException(nameof(RequestReceivedEventArgs.Request));
			var message = request.ToString();
			var startDate = DateTime.UtcNow;

			var ctx = new RequestContext();
			try
			{
				_logger?.Verbose("Received message from server. connection-id={ConnectionId}, message={Message}", args.ConnectionId, message);

				try
				{
					if (request.IsConfigurationRequest)
					{
						await HandleConfigurationRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}

					if (request.IsPingRequest)
					{
						await HandlePingRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}

					if (request.IsHeartbeatRequest)
					{
						await HandleHeartbeatRequestAsync(ctx, request).ConfigureAwait(false);
						return;
					}
				}
				finally
				{
					await AcknowledgeRequestAsync(request).ConfigureAwait(false);
				}

				var key = request.Url.Split('/').FirstOrDefault();
				if (key != null)
				{
					if (_connectors.TryGetValue(key, out var connector))
					{
						_logger?.Verbose("Found on-premise target and sending request. request-id={RequestId}, on-premise-key={OnPremiseTargetKey}", request.RequestId, key);

						LastActivity = DateTime.UtcNow;

						await RequestLocalTargetAsync(ctx, key, connector, request, CancellationToken.None).ConfigureAwait(false); // TODO no cancellation token here?
						return;
					}
				}

				_logger?.Information("No connector found for local server for request {RequestId} and url {RequestUrl}", request.RequestId, request.Url);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during handling received message. connection-id={ConnectionId}, message={Message}", args.ConnectionId, message);
			}
			finally
			{
				if (!ctx.IsRelayServerNotified)
				{
					_logger?.Warning("Unhandled request. connection-id={ConnectionId}, request-id={RequestId}, message={message}", args.ConnectionId, request.RequestId, message);

					var response = new OnPremiseTargetResponse()
					{
						RequestStarted = startDate,
						RequestFinished = DateTime.UtcNow,
						StatusCode = HttpStatusCode.NotFound,
						OriginId = request.OriginId,
						RequestId = request.RequestId,
						HttpHeaders = new Dictionary<string, string>(),
					};

					try
					{
						// No cancellation token here, to not cancel sending of an already fetched response
						await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during posting unhandled request response to relay. request-id={RequestId}", request.RequestId);
					}
				}
			}
		}

		private async Task HandleConfigurationRequestAsync(RequestContext ctx, IOnPremiseTargetRequestInternal request)
		{
			_logger?.Debug("Received configuration request from RelayServer. request-id={RequestId}", request.RequestId);

			using (var sr = new JsonTextReader(new StreamReader(new MemoryStream(request.Body))))
			{
				var config = _jsonSerializer.Deserialize<ServerSideLinkConfiguration>(sr);

				TokenRefreshWindow = config.TokenRefreshWindow ?? TokenRefreshWindow;
				HeartbeatInterval = config.HeartbeatInterval ?? HeartbeatInterval;

				_minReconnectWaitTime = config.ReconnectMinWaitTime ?? _minReconnectWaitTime;
				_maxReconnectWaitTime = config.ReconnectMaxWaitTime ?? _maxReconnectWaitTime;
				_logSensitiveData = config.LogSensitiveData ?? _logSensitiveData;

				AbsoluteConnectionLifetime = config.AbsoluteConnectionLifetime ?? AbsoluteConnectionLifetime;
				SlidingConnectionLifetime = config.SlidingConnectionLifetime ?? SlidingConnectionLifetime;

				_logger?.Debug("Applied configuration from RelayServer. configuration={@Configuration}", config);
			}

			var response = BuildDefaultResponse(request.OriginId, request.RequestId);

			try
			{
				// No cancellation token here, to not cancel sending of an already fetched response
				await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during posting configuration response to relay. request-id={RequestId}", request.RequestId);
			}
		}

		private async Task HandlePingRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received ping from RelayServer. request-id={RequestId}", request.RequestId);
			var response = BuildDefaultResponse(request.OriginId, request.RequestId);

			try
			{
				// No cancellation token here, to not cancel sending of an already fetched response
				await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during posting ping response to relay. request-id={RequestId}", request.RequestId);
			}
		}

		private async Task HandleHeartbeatRequestAsync(RequestContext ctx, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Received heartbeat from RelayServer. request-id={RequestId}", request.RequestId);

			if (LastHeartbeat == DateTime.MinValue)
			{
				if (request.HttpHeaders != null)
				{
					request.HttpHeaders.TryGetValue("X-TTRELAY-HEARTBEATINTERVAL", out var heartbeatHeaderValue);
					if (Int32.TryParse(heartbeatHeaderValue, out var heartbeatInterval))
					{
						_logger?.Information("Heartbeat interval set to {HeartbeatInterval} seconds", heartbeatInterval);
						HeartbeatInterval = TimeSpan.FromSeconds(heartbeatInterval);
					}
				}
			}

			LastHeartbeat = DateTime.UtcNow;
			var response = BuildDefaultResponse(request.OriginId, request.RequestId);

			try
			{
				// No cancellation token here, to not cancel sending of an already fetched response
				await PostResponseAsync(ctx, response, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during posting heartbeat response to relay. request-id={RequestId}", request.RequestId);
			}
		}

		private IOnPremiseTargetResponse BuildDefaultResponse(Guid originId, string requestId)
		{
			return new OnPremiseTargetResponse()
			{
				RequestStarted = DateTime.UtcNow,
				RequestFinished = DateTime.UtcNow,
				StatusCode = HttpStatusCode.OK,
				OriginId = originId,
				RequestId = requestId,
			};
		}

		private async Task RequestLocalTargetAsync(RequestContext ctx, string key, IOnPremiseTargetConnector connector, IOnPremiseTargetRequestInternal request, CancellationToken cancellationToken)
		{
			var uri = new Uri(new Uri("http://local-target/"), request.Url);
			_logger?.Debug("Relaying request to local target. request-id={RequestId}, request-url={RequestUrl}", request.RequestId, _logSensitiveData ? uri.PathAndQuery : uri.AbsolutePath);

			var url = (request.Url.Length > key.Length) ? request.Url.Substring(key.Length + 1) : String.Empty;

			if (request.Body != null)
			{
				if (request.Body.Length == 0)
				{
					// a length of 0 indicates that there is a larger body available on the server
					_logger?.Verbose("Requesting body. request-id={RequestId}", request.RequestId);
					// request the body from the RelayServer (because SignalR cannot handle large messages)
					var webResponse = await GetToRelayAsync("/request/" + request.RequestId, cancellationToken).ConfigureAwait(false);
					request.Stream = await webResponse.Content.ReadAsStreamAsync().ConfigureAwait(false); // this stream should not be disposed (owned by the Framework)
				}
				else
				{
					// the body is small enough to be used directly
					request.Stream = new MemoryStream(request.Body);
				}
			}
			else
			{
				// no body available (e.g. GET request)
				request.Stream = Stream.Null;
			}

			if (request.Stream.Position != 0 && request.Stream.CanSeek)
			{
				request.Stream.Position = 0;
			}

			using (var response = await GetResponseFromLocalTargetAsync(connector, url, request, RelayedRequestHeader).ConfigureAwait(false))
			{
				_logger?.Debug("Sending response. request-id={RequestId}", request.RequestId);

				try
				{
					// transfer the result to the RelayServer (need POST here, because SignalR does not handle large messages)
					await PostResponseAsync(ctx, response, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Error during posting local target response to relay. request-id={RequestId}", request.RequestId);
				}
			}
		}

		private async Task<InterceptedResponse> GetResponseFromLocalTargetAsync(IOnPremiseTargetConnector connector, string url, IOnPremiseTargetRequest onPremiseTargetRequest, string relayedRequestHeader)
		{
			var request= new InterceptedRequest(onPremiseTargetRequest);

			try
			{
				var requestInterceptor = _onPremiseInterceptorFactory.CreateOnPremiseRequestInterceptor();
				requestInterceptor?.HandleRequest(request);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during intercepting the request. request-id={RequestId}", request.RequestId);
			}

			var onPremiseTargetResponse = await connector.GetResponseFromLocalTargetAsync(url, request, relayedRequestHeader).ConfigureAwait(false);
			var response = new InterceptedResponse(onPremiseTargetResponse);

			try
			{
				var responseInterceptor = _onPremiseInterceptorFactory.CreateOnPremiseResponseInterceptor();
				responseInterceptor?.HandleResponse(request, response);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during intercepting the response. request-id={RequestId}", request.RequestId);
			}

			return response;
		}

		private async Task PostResponseAsync(RequestContext ctx, IOnPremiseTargetResponse response, CancellationToken cancellationToken)
		{
			ctx.IsRelayServerNotified = true;
			await PostToRelayAsync("/forward", headers => headers.Add("X-TTRELAY-METADATA", JsonConvert.SerializeObject(response)), new StreamContent(response.Stream ?? Stream.Null, 0x10000), cancellationToken).ConfigureAwait(false);
		}

		public Task<HttpResponseMessage> GetToRelayAsync(string relativeUrl, CancellationToken cancellationToken)
		{
			return GetToRelayAsync(relativeUrl, null, cancellationToken);
		}

		public Task<HttpResponseMessage> GetToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken)
		{
			return _httpConnection.SendToRelayAsync(relativeUrl, HttpMethod.Get, setHeaders, null, cancellationToken);
		}

		public Task<HttpResponseMessage> PostToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			return _httpConnection.SendToRelayAsync(relativeUrl, HttpMethod.Post, setHeaders, content, cancellationToken);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			OnDisposing();

			if (disposing)
			{
				if (_cts != null)
				{
					_cts.Cancel();
					_cts.Dispose();
					_cts = null;
				}

				_httpConnection.Dispose();
				_signalrClient?.Dispose();
			}
		}

		protected virtual void OnDisposing()
		{
			Disposing?.Invoke(this, EventArgs.Empty);
		}
	}
}
