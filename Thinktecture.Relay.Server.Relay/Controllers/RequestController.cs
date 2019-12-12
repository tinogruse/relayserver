using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Relay.Services;

namespace Thinktecture.Relay.Server.Relay.Controllers
{
	[Route("[controller]")]
	public class RequestController : OnPremisesControllerBase
	{
		private readonly ILogger<RequestController> _logger;
		private readonly IRequestResponseBodyStore _bodyStore;

		public RequestController(ILogger<RequestController> logger, IRequestResponseBodyStore bodyStore)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
		}

		[HttpGet("{requestId}")]
		public async Task<ActionResult> GetRequestBody(Guid requestId)
		{
			// Todo: We should store the link id with the body, so that only the one link that request is targeted at can load it.
			var data = await _bodyStore.GetBodyAsync(Guid.Empty, requestId).ConfigureAwait(false);
			if (data != null)
			{
				return File(data, "application/octet-stream", requestId.ToString());
			}

			return NotFound();
		}


		[Route("[action]")]
		public async Task<ActionResult> Acknowledge([FromQuery(Name = "oid")] string originId, [FromQuery(Name = "aid")] string acknowledgeId, [FromQuery(Name = "cid")] string connectionId = null)
		{
			_logger.LogInformation("Request got acknowledged. Acknowledge Id {aid}, Origin Id {oid}, Connection Id {cid}", acknowledgeId, originId, connectionId);

			// Todo: await _queue.SendAcknowledgeAsync(originId, acknowledgeId, connectionId).ConfigureAwait(false);

			return Ok();
		}
	}
}
