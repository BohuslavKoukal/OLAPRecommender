namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "User_Id", c => c.String(maxLength: 128, storeType: "nvarchar"));
            CreateIndex("dbo.Dataset", "User_Id");
            AddForeignKey("dbo.Dataset", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Dataset", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Dataset", new[] { "User_Id" });
            DropColumn("dbo.Dataset", "User_Id");
        }
    }
}
