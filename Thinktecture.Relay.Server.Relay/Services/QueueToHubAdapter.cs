using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Relay.Hubs;

namespace Thinktecture.Relay.Server.Relay.Services
{
	public class QueueToHubAdapter : BackgroundService
	{
		private readonly IRequestResponseQueue _queue;
		private readonly IHubContext<OnPremisesHub, IOnPremisesClient> _hub;

		public QueueToHubAdapter(IRequestResponseQueue queue, IHubContext<OnPremisesHub, IOnPremisesClient> hub)
		{
			_queue = queue;
			_hub = hub;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using (_queue.Requests.Subscribe(r => _hub.Clients.All.ReceiveRequest(r)))
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
				}
			}
		}
	}
}
