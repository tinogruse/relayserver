using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Relay.Hubs;
using Thinktecture.Relay.Server.Relay.Middlewares;
using Thinktecture.Relay.Server.Relay.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing
{
	public static class EndpointRouteBuilderExtensions
	{
		public static void MapRelayServer(this IEndpointRouteBuilder endpoints)
		{
			var config = endpoints.ServiceProvider.GetRequiredService<IOptions<RelayServerOptions>>().Value;

			endpoints.MapMiddleware<RelayingMiddleware>(BuildRelayRouteTemplate(config));
			endpoints.MapHub<OnPremisesHub>(config.AbsoluteConnectorPath);

			endpoints.MapAreaControllerRoute("OnPremisesControllers", "OnPremises", BuildOnPremisesRouteTemplate(config));
		}

		public static void MapMiddleware<T>(this IEndpointRouteBuilder endpoints, string pattern)
		{
			var pipeline = endpoints
				.CreateApplicationBuilder()
				.UseMiddleware<T>()
				.Build();

			endpoints.Map(pattern, pipeline);
		}

		private static string BuildRelayRouteTemplate(RelayServerOptions config)
		{
			var absoluteRelayPath = config.AbsoluteRelayPath.TrimEnd('/');

			return absoluteRelayPath + "/{LinkName}/{TargetName}/{*OnPremisesTargetPath}";
		}

		private static string BuildOnPremisesRouteTemplate(RelayServerOptions config)
		{
			var absoluteOnPremisesPath = config.AbsoluteOnPremisesPath.TrimEnd('/');

			return absoluteOnPremisesPath + "/{controller}/{action?}/{id?}";
		}

	}
}
