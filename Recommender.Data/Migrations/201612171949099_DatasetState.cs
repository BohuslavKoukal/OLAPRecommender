using System.Data.Entity.Migrations;

namespace Recommender.Data.Migrations
{
    public partial class DatasetState : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "State", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "State");
        }
    }
}
