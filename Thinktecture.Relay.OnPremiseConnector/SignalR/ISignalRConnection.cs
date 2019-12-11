using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface ISignalRConnection: IDisposable
	{
		event Action Reconnecting;
		event Action Reconnected;
		event EventHandler<RequestReceivedEventArgs> RequestReceived;
		event Action ConnectionClosed;

		string ConnectionId { get; }
		KeyValuePair<string, string> AccessToken { set; }

		Task Start();
		void Stop();
	}
}
