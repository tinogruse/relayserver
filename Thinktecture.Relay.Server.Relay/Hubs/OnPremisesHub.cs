using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Server.Relay.Models;

namespace Thinktecture.Relay.Server.Relay.Hubs
{
	public interface IOnPremisesClient
	{
		Task ReceiveRequest(OnPremiseRequest request);
	}

	public class OnPremisesHub : Hub<IOnPremisesClient>
	{
		public async Task SendRequestAsync(string linkName, OnPremiseRequest request)
		{
			await Clients.All.ReceiveRequest(request);
		}
	}
}
