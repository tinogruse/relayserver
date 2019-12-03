using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.Server.Relay.Controllers
{
	/// <summary>
	/// Intended for derivation of other OnPremises controllers, as they should
	/// share the same route and authorization information.
	/// </summary>
	[ApiController, Area("OnPremises"), Route("[controller]")]
	public abstract class OnPremisesControllerBase : ControllerBase
	{
	}
}
