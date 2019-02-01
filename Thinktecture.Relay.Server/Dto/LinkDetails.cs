using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkDetails
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public string DisplayName { get; set; }
		public bool IsDisabled { get; set; }
		public int MaximumConnections { get; set; }
		public DateTime CreationDate { get; set; }
		public bool AllowLocalClientRequestsOnly { get; set; }
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }

		public int ConnectionCount => Connections.Count;

		private List<LinkConnection> _connections;
		public List<LinkConnection> Connections
		{
			get => _connections ?? (_connections = new List<LinkConnection>());
			set => _connections = value;
		}

		public TimeSpan? TokenRefreshWindow { get; set; }
		public TimeSpan? HeartbeatInterval { get; set; }
		public TimeSpan? ReconnectMinWaitTime { get; set; }
		public TimeSpan? ReconnectMaxWaitTime { get; set; }

		public TimeSpan? AbsoluteConnectionLifetime { get; set; }
		public TimeSpan? SlidingConnectionLifetime { get; set; }
	}
}
