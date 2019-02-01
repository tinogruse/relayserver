using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkRelayInfo
	{
		public Guid Id { get; set; }
		public string DisplayName { get; set; }
		public bool IsDisabled { get; set; }
		public bool AllowLocalClientRequestsOnly { get; set; }
		public bool ForwardOnPremiseTargetErrorResponse { get; set; }
	}
}
