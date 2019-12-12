using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Relay.Models
{
	public class RelayedResponse
	{
		public Guid RequestId { get; set; }
		public Guid OriginId { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public int StatusCode { get; set; }
		public Dictionary<string, string> HttpHeaders { get; set; }
	}
}
