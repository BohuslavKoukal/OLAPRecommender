namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class privatefile : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Dataset", "KeepFilePrivate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Dataset", "KeepFilePrivate", c => c.Boolean(nullable: false));
        }
    }
}
