namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserChange : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Dataset", "User_Id", "AspNetUsers");
            DropIndex("Dataset", new[] { "User_Id" });
            AddColumn("Dataset", "UserId", c => c.String(unicode: false));
            DropColumn("Dataset", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("Dataset", "User_Id", c => c.String(maxLength: 128, storeType: "nvarchar"));
            DropColumn("Dataset", "UserId");
            CreateIndex("Dataset", "User_Id");
            AddForeignKey("Dataset", "User_Id", "AspNetUsers", "Id");
        }
    }
}
