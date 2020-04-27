using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.EntityFramework.Entities;
using Thinktecture.Relay.Server.EntityFramework.Extensions;

namespace Thinktecture.Relay.Server.EntityFramework.DbContexts
{
	/// <summary>
	/// Provides EntityFramework Core data access logic for the RelayServer
	/// </summary>
	public class RelayServerConfigurationDbContext: DbContext
	{
		/// <summary>
		/// The tenants (formerly known as links) that can connect with their connectors.
		/// </summary>
		public DbSet<Tenant> Tenants { get; set; }

		/// <summary>
		/// The client secrets a connector needs to provide when connecting to the RelayServer.
		/// </summary>
		public DbSet<ClientSecret> ClientSecrets{ get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayServerConfigurationDbContext"/>.
		/// </summary>
		/// <param name="options"></param>
		public RelayServerConfigurationDbContext(DbContextOptions<RelayServerConfigurationDbContext> options)
			: base(options)
		{
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ConfigureConfigurationEntities();
		}
	}
}
