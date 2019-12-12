using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Core
{
	internal class SignalRConnection : ISignalRConnection
	{
		public event Action Reconnecting;
		public event Action Reconnected;
		public event EventHandler<RequestReceivedEventArgs> RequestReceived;
		public event Action ConnectionClosed;

		public string ConnectionId { get; set; }
		public KeyValuePair<string, string> AccessToken { get; set; }

		private HubConnection _connection;

		public SignalRConnection(ILogger logger, Uri relayServerUri, Assembly versionAssembly, int connectorVersion, string tokenType, string accessToken)
		{
			AccessToken = new KeyValuePair<string, string>(tokenType, accessToken);

			_connection = new HubConnectionBuilder()
				.WithUrl($"{relayServerUri}onpremises", options => {
					options.AccessTokenProvider = () => Task.FromResult(AccessToken.Value);
					options.Headers = new Dictionary<string, string>()
					{
						{ "TTRelay-ConnectorVersion", connectorVersion.ToString() },
						{ "TTRelay-VersionAssembly", versionAssembly.GetName().Version.ToString() },
					};
				})
				.WithAutomaticReconnect()
				.ConfigureLogging(lb => lb.AddSerilog())
				.Build();

			_connection.Reconnecting += (ex) =>
			{
				logger.Information("SignalR connection is reconnecting.");
				Reconnecting?.Invoke();
				return Task.CompletedTask;
			};

			_connection.Reconnected += (connectionId) =>
			{
				logger.Information("SignalR connection has reconnected.");
				Reconnected?.Invoke();
				return Task.CompletedTask;
			};

			_connection.Closed += (ex) =>
			{
				logger.Information("SignalR connection closed.");
				ConnectionClosed?.Invoke();
				return Task.CompletedTask;
			};

			_connection.On<OnPremiseTargetRequest>("ReceiveRequest", (request) =>
			{
				logger.Information("SignalR connection received request.");
				RequestReceived?.Invoke(this, new RequestReceivedEventArgs(request, _connection.ConnectionId));
			});
		}

		public Task Start()
		{
			return _connection.StartAsync();
		}

		public void Stop()
		{
			_connection.StopAsync().RunSynchronously();
		}

		public void Dispose()
		{
			_connection.DisposeAsync();
		}
	}
}
