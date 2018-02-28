namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatasetPreprocessed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "MinerId", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "MinerId");
        }
    }
}
