using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Relay.Options;
using Thinktecture.Relay.Server.Relay.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRelayServer(this IServiceCollection services, IConfiguration configuration)
		{
			// Todo: Make configurable
			services.TryAddSingleton<IJsonSerializer, CustomJsonSerializer>();
			services.TryAddSingleton<IRequestResponseBodyStore, InMemoryRequestResponseBodyStore>();
			services.TryAddSingleton<IRequestResponseQueue, InMemoryRequestResponseQueue>();

			services.AddScoped<IRequestDispatcher, RequestDispatcher>();

			services.AddTransient<IValidateOptions<RelayServerOptions>, RelayServerOptionsValidator>();

			services.Configure<RelayServerOptions>(configuration.GetSection("RelayServer"));

			services.AddHostedService<QueueToHubAdapter>();
			services.AddMvcCore().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);
			services.AddSignalR();

			return services;
		}
	}
}
