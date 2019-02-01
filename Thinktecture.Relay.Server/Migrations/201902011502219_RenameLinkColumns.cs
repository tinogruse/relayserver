namespace Thinktecture.Relay.Server.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameLinkColumns : DbMigration
    {
        public override void Up()
        {
			RenameColumn("dbo.Links", "SymbolicName", "DisplayName");
			RenameColumn("dbo.Links", "MaximumLinks", "MaximumConnections");
        }
        
        public override void Down()
        {
			RenameColumn("dbo.Links", "DisplayName", "SymbolicName");
			RenameColumn("dbo.Links", "MaximumConnections", "MaximumLinks");
        }
    }
}
