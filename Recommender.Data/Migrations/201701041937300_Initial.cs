namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        DataSet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dataset", t => t.DataSet_Id)
                .Index(t => t.DataSet_Id);
            
            CreateTable(
                "dbo.Dataset",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        CsvFilePath = c.String(unicode: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Dimension",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Type = c.Int(nullable: false),
                        DataSet_Id = c.Int(),
                        ParentDimension_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dataset", t => t.DataSet_Id)
                .ForeignKey("dbo.Dimension", t => t.ParentDimension_Id)
                .Index(t => t.DataSet_Id)
                .Index(t => t.ParentDimension_Id);
            
            CreateTable(
                "dbo.Measure",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        RdfUri = c.String(unicode: false),
                        Type = c.Int(nullable: false),
                        DataSet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dataset", t => t.DataSet_Id)
                .Index(t => t.DataSet_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Measure", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Dimension", "ParentDimension_Id", "dbo.Dimension");
            DropForeignKey("dbo.Dimension", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Attribute", "DataSet_Id", "dbo.Dataset");
            DropIndex("dbo.Measure", new[] { "DataSet_Id" });
            DropIndex("dbo.Dimension", new[] { "ParentDimension_Id" });
            DropIndex("dbo.Dimension", new[] { "DataSet_Id" });
            DropIndex("dbo.Attribute", new[] { "DataSet_Id" });
            DropTable("dbo.Measure");
            DropTable("dbo.Dimension");
            DropTable("dbo.Dataset");
            DropTable("dbo.Attribute");
        }
    }
}
