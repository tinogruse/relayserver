using System;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkConnection
	{
		public string Id { get; set; }
		public int ProtocolVersion { get; set; }
		public string AssemblyVersion { get; set; }
		public DateTime LastActivity { get; set; }
		public bool IsStalled { get; set; }
	}
}
