using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server.Relay.Models
{
	public class OnPremiseRequest
	{
		public Guid RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string HttpMethod { get; set; }
		public DateTime RequestStarted { get; set; } = DateTime.UtcNow;
		public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(30);
		public long ContentLength { get; set; }
		public string Url { get; set; }
		public Dictionary<string, string[]> HttpHeaders { get; set; }

		public OnPremiseRequest(Guid requestId, HttpRequest request, string path)
		{
			RequestId = requestId;
			HttpMethod = request.Method;
			Url = path;
			HttpHeaders = request.Headers
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
			OriginId = Guid.Empty;
			ContentLength = request.GetTypedHeaders().ContentLength ?? 0;

			HttpHeaders.Remove("Host");
			HttpHeaders.Remove("Connection");
		}
	}
}
