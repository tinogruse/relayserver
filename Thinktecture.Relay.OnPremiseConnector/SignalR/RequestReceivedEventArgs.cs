using System;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class RequestReceivedEventArgs : EventArgs
	{
		public IOnPremiseTargetRequestInternal Request { get; }
		public string ConnectionId { get; }

		public RequestReceivedEventArgs(IOnPremiseTargetRequestInternal request, string connectionId)
		{
			Request = request ?? throw new ArgumentNullException(nameof(request));
			ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
		}
	}
}
