using Microsoft.AspNetCore.Mvc;

namespace Thinktecture.Relay.Server.Service.Controllers
{
	[ApiController, Route("{controller}")]
	public class TestController : ControllerBase
	{
		[HttpGet]
		public string Get()
		{
			return "Hello world";
		}
	}
}
