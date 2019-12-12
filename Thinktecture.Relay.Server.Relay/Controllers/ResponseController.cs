using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Relay.Models;
using Thinktecture.Relay.Server.Relay.Services;

namespace Thinktecture.Relay.Server.Relay.Controllers
{
	[Route("")]
	public class ResponseController : OnPremisesControllerBase
	{
		private readonly ILogger<ResponseController> _logger;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly IRequestResponseBodyStore _bodyStore;
		private readonly IRequestResponseQueue _queue;

		public ResponseController(ILogger<ResponseController> logger, IJsonSerializer jsonSerializer, IRequestResponseBodyStore bodyStore, IRequestResponseQueue queue)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_queue = queue ?? throw new ArgumentNullException(nameof(queue));
		}

		[HttpPost("/forward")]
		public async Task<ActionResult> PostResponse()
		{
			_logger.LogInformation("RESPONSECONTROLLER Received response.");

			var responseString = Request.Headers["X-TTRELAY-METADATA"];
			var response = // _jsonSerializer.Deserialize<RelayedResponse>(responseString);
				JsonSerializer.Deserialize<RelayedResponse>(responseString);

			if (Request.Body != null)
			{
				await _bodyStore.StoreBodyAsync(Guid.Empty, response.RequestId, Request.Body).ConfigureAwait(false);
			}

			_queue.SendResponse(response);

			return Ok();
		}
	}
}
