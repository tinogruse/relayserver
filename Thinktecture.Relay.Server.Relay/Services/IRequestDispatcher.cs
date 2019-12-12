using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server.Relay.Services
{
	/// <summary>
	/// Dispatches requests to the internal Relay handling services.
	/// </summary>
	public interface IRequestDispatcher
	{
		/// <summary>
		/// Dispatches the request to the OPC.
		/// </summary>
		/// <param name="httpContext">The http context for the current request.</param>
		/// <returns></returns>
		Task<HttpResponseMessage> DispatchRequestAsync(HttpContext httpContext);
	}
}
