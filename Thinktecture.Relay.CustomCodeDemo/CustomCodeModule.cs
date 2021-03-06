using Autofac;
using Autofac.Integration.WebApi;
using Thinktecture.Relay.Server.Interceptor;

namespace Thinktecture.Relay.CustomCodeDemo
{
	/// <inheritdoc />
	/// <summary>
	/// A RelayServer custom code assembly may provide a single Autofac module, that will register all
	/// types that are implemented and should be used.
	/// </summary>
	public class CustomCodeModule : Module
	{
		/// <inheritdoc />
		/// <summary>
		/// Override the Load method of the Autofac module to register the types.
		/// </summary>
		/// <param name="builder"></param>
		protected override void Load(ContainerBuilder builder)
		{
			// Each interceptor that should be added needs to be registered with the container builder as its interface type
			builder.RegisterType<DemoRequestInterceptor>().As<IOnPremiseRequestInterceptor>();
			builder.RegisterType<DemoResponseInterceptor>().As<IOnPremiseResponseInterceptor>();

			builder.RegisterApiControllers(typeof(CustomCodeModule).Assembly);

			// Example: Override the password complexity validator with a custom version
			builder.RegisterType<NoopPasswordComplexityValidator>().AsImplementedInterfaces();

			// Example: Override request logger with a custom version
			builder.RegisterType<NoopRequestLogger>().AsImplementedInterfaces();

			base.Load(builder);
		}
	}
}
