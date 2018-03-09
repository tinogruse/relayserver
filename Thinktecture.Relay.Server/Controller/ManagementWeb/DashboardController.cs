using System.Web.Http;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Http.Filters;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Controller.ManagementWeb
{
	[Authorize(Roles = "Admin")]
	[ManagementWebModuleBindingFilter]
	[NoCache]
	public class DashboardController : ApiController
	{
		private readonly ILogRepository _logRepository;

		public DashboardController(ILogRepository logRepository)
		{
			_logRepository = logRepository;
		}

		[HttpGet]
		[ActionName("info")]
		public IHttpActionResult Get(int numberOfEntries = 15, int numberOfDays = 7)
		{
			var result = new Dashboard()
			{
				Logs = _logRepository.GetRecentLogEntries(numberOfEntries),
				Chart = _logRepository.GetContentBytesChartDataItems(numberOfDays)
			};

			return Ok(result);
		}
	}
}
