using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Thinktecture.Relay.Server.Relay.Models;

namespace Thinktecture.Relay.Server.Relay.Controllers
{
	public class VersionController : OnPremisesControllerBase
	{
		[HttpGet]
		public ActionResult<RelayServerVersion> GetVersion()
		{
			return new RelayServerVersion() {
				RelayVersion = GetVersionFromAssembly(GetType().Assembly),
				HostVersion = GetVersionFromAssembly(Assembly.GetEntryAssembly()),
			};
		}

		private string GetVersionFromAssembly(Assembly assembly)
		{
			return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
				?? assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
				?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		}
	}
}
