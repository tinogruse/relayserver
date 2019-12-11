using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class SignalRConnection : Connection, ISignalRConnection
	{
		private readonly ILogger _logger;

		public event EventHandler<RequestReceivedEventArgs> RequestReceived;
		public event Action ConnectionClosed;

		public KeyValuePair<string, string> AccessToken
		{
			set => Headers["Authorization"] = $"{value.Key} {value.Value}";
		}

		public SignalRConnection(ILogger logger, Uri relayServerUri, Assembly versionAssembly, int connectorVersion, int relayServerConnectionInstanceId)
			: base(new Uri(relayServerUri, "/signalr").AbsoluteUri, $"cv={connectorVersion}&av={versionAssembly.GetName().Version}")
		{
			_logger = logger;
		}

		protected override void OnMessageReceived(JToken message)
		{
			IOnPremiseTargetRequestInternal request = message.ToObject<OnPremiseTargetRequest>();
			request.ConnectionId = ConnectionId;
			RequestReceived?.Invoke(this, new RequestReceivedEventArgs(request, ConnectionId));
		}

		protected override void OnClosed()
		{
			ConnectionClosed?.Invoke();
			base.OnClosed();
		}
	}
}
