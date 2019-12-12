using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server.Relay.Models
{
	public class OnPremiseRequest
	{
		public Guid RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public DateTime RequestStarted { get; set; } = DateTime.UtcNow;
		public DateTime RequestFinished { get; set; } = DateTime.UtcNow;
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public Dictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public AcknowledgmentMode AcknowledgmentMode { get; set; } = AcknowledgmentMode.Default;
		
		[JsonIgnore]
		public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(30);
		[JsonIgnore]
		public long ContentLength { get; set; }

		public OnPremiseRequest(Guid requestId, HttpRequest request, string path)
		{
			RequestId = requestId;
			HttpMethod = request.Method;
			Url = path;
			HttpHeaders = request.Headers
				.ToDictionary(
					kvp => kvp.Key,
					kvp => String.Join(';', kvp.Value.ToArray())
				);
			OriginId = Guid.Empty;
			AcknowledgeId = RequestId.ToString();
			ContentLength = request.GetTypedHeaders().ContentLength ?? 0;
			Url = request.Path.Value.Replace("/relay/test/", String.Empty) + request.QueryString;
			
			HttpHeaders.Remove("Host");
			HttpHeaders.Remove("Connection");
		}
	}
}
