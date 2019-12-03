using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Thinktecture.Relay.Server.Service
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			try
			{
				Log.Logger = new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.Enrich.FromLogContext()
					.Enrich.WithProperty("Application", "RelayServer Core")
					.WriteTo.Console()
					.CreateLogger();

				CreateHostBuilder(args).Build().Run();
				return 0;
			}
			catch (Exception ex)
			{
				Log.Logger.Fatal(ex, "A fatal error cause the service to terminate.");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			 Host.CreateDefaultBuilder(args)
				.ConfigureServices(services =>
				{
					services.Configure<ConsoleLifetimeOptions>(options =>
					{
						options.SuppressStatusMessages = true;
					});
				 })
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.UseSerilog();
	}
}
