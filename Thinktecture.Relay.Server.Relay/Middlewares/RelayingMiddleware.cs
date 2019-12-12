using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Relay.Options;
using Thinktecture.Relay.Server.Relay.Services;

namespace Thinktecture.Relay.Server.Relay.Middlewares
{
	public class RelayingMiddleware
	{
		private readonly RelayServerOptions _configuration;

		public RelayingMiddleware(RequestDelegate _, IOptions<RelayServerOptions> options)
		{
			_configuration = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public async Task Invoke(HttpContext httpContext)
		{
			var requestDispatcher = httpContext.RequestServices.GetRequiredService<IRequestDispatcher>();
			var logger = httpContext.RequestServices.GetRequiredService<ILogger<RelayingMiddleware>>();

			logger.LogDebug("MIDDLEWARE received request. Dispatching...");

			using var response = await requestDispatcher.DispatchRequestAsync(httpContext).ConfigureAwait(false);

			logger.LogDebug("MIDDLEWARE received response. Relaying...");

			await CopyResponseAsync(httpContext, response).ConfigureAwait(false);
		}

		private async Task CopyResponseAsync(HttpContext context, HttpResponseMessage responseMessage)
		{
			var response = context.Response;

			response.StatusCode = (int)responseMessage.StatusCode;
			response.Headers.CopyFrom(responseMessage.Headers);
			response.Headers.CopyFrom(responseMessage?.Content?.Headers);

			// SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
			response.Headers.Remove("transfer-encoding");

			if (responseMessage.Content != null)
			{
				var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
				await responseStream.CopyToAsync(response.Body, _configuration.StreamCopyBufferSize, context.RequestAborted).ConfigureAwait(false);
				if (responseStream.CanWrite)
				{
					await responseStream.FlushAsync(context.RequestAborted).ConfigureAwait(false);
				}
			}
		}
	}
}
