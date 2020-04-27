using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Server.EntityFramework.DbContexts;

namespace Thinktecture.Relay.Server.EntityFramework.MigrationCreation.SqlServer
{
	public class DbContextFactory : IDesignTimeDbContextFactory<RelayServerConfigurationDbContext>
	{
		public RelayServerConfigurationDbContext CreateDbContext(string[] args)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<RelayServerConfigurationDbContext>();

			builder.UseSqlServer(
				configuration.GetConnectionString("SqlServer"),
				optionsBuilder =>
				{
					optionsBuilder.MigrationsAssembly("Thinktecture.Relay.Server.EntityFramework.SqlServer");
				});

			return new RelayServerConfigurationDbContext(builder.Options);
		}
	}
}
