using System;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Relay.Models;

namespace Thinktecture.Relay.Server.Relay.Services
{
	public class RequestDispatcher : IRequestDispatcher
	{
		private readonly ILogger<RequestDispatcher> _logger;
		private readonly IRequestResponseBodyStore _bodyStore;
		private readonly IRequestResponseQueue _queue;

		public RequestDispatcher(ILogger<RequestDispatcher> logger, IRequestResponseBodyStore bodyStore, IRequestResponseQueue queue)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_queue = queue ?? throw new ArgumentNullException(nameof(queue));
		}

		public async Task<HttpResponseMessage> DispatchRequestAsync(HttpContext httpContext)
		{
			var link = httpContext.GetRouteValue("LinkName").ToString();
			var target = httpContext.GetRouteValue("TargetName").ToString();
			var path = httpContext.GetRouteValue("OnPremisesTargetPath")?.ToString()
				+ httpContext.Request.QueryString;

			_logger.LogInformation("DISPATCHER Received request for Link {link} and Target {target} with Path {path}", link, target, path);

			// Todo: Check if Link is active and has this target registered. If not, we can directly abort here and return 
			// Todo: Either return 503 ("Service Unavailable") or 523 (Cloudflare status code for "Origin is Unreachable").

			var requestId = new Guid("affeaffe-affe-affe-affe-affeaffeaffe");

			// Store request body
			if (httpContext.Request.Body != null)
			{
				await _bodyStore.StoreBodyAsync(Guid.Empty, requestId, httpContext.Request.Body);
			}

			var request = new OnPremiseRequest(requestId, httpContext.Request, path) { OriginId = new Guid("deadbeef-dead-dead-beef-deadbeefdead") };

			_logger.LogInformation("DISPATCHER Sending request {@request}", request);

			// Start listening for a response
			var responseTask = _queue.Responses
				.Where(r => r.RequestId == requestId)
				.Timeout(TimeSpan.FromMinutes(30)) // Todo: Use configurable timeout
				.FirstOrDefaultAsync();

			// Send request
			_queue.SendRequest(request);

			// await response from queue
			var response = await responseTask;

			// if we get one fetch body and prepare result
			if (response != null)
			{
				var result = new HttpResponseMessage((HttpStatusCode)response.StatusCode);

				var body = await _bodyStore.GetBodyAsync(Guid.Empty, requestId);
				if (body != null)
				{
					result.Content = new StreamContent(body);
				}

				foreach (var header in response.HttpHeaders)
				{
					try
					{
						if (header.Key.Contains("Content", StringComparison.InvariantCultureIgnoreCase))
						{
							result.Content.Headers.Add(header.Key, header.Value);
						}
						else
						{
							result.Headers.Add(header.Key, header.Value);
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "An error occured while copying http header {Header} with value {value}", header.Key, header.Value);
					}
				}

				return result;
			}

			return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
		}
	}
}
