using System.Web.Http;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[AllowAnonymous]
	[ManagementWebModuleBindingFilter]
	[NoCache]
	public class SetupController : ApiController
	{
		private readonly IUserRepository _userRepository;

		public SetupController(IUserRepository userRepository)
		{
			_userRepository = userRepository;
		}

		[HttpGet]
		public IHttpActionResult NeedsFirstTimeSetup()
		{
			return Ok(new { Setup = !_userRepository.Any() });
		}
	}
}
