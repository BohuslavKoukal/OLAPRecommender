namespace Recommender.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AssociationRule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Base = c.Double(nullable: false),
                        Aad = c.Double(nullable: false),
                        Text = c.String(unicode: false),
                        MiningTask_Id = c.Int(),
                        SuccedentMeasure_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MiningTask", t => t.MiningTask_Id)
                .ForeignKey("dbo.Measure", t => t.SuccedentMeasure_Id)
                .Index(t => t.MiningTask_Id)
                .Index(t => t.SuccedentMeasure_Id);
            
            CreateTable(
                "dbo.DimensionValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(unicode: false),
                        Dimension_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dimension", t => t.Dimension_Id)
                .Index(t => t.Dimension_Id);
            
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
            
            CreateTable(
                "dbo.MiningTask",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        NumberOfVerifications = c.Int(nullable: false),
                        TaskState = c.Int(nullable: false),
                        Base = c.Double(nullable: false),
                        Aad = c.Double(nullable: false),
                        TaskStartTime = c.DateTime(nullable: false, precision: 0),
                        TaskDuration = c.Time(nullable: false, precision: 0),
                        DataSet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dataset", t => t.DataSet_Id)
                .Index(t => t.DataSet_Id);
            
            CreateTable(
                "dbo.AntecedentDimensionValues",
                c => new
                    {
                        AssociationRuleId = c.Int(nullable: false),
                        DimensionValueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AssociationRuleId, t.DimensionValueId })
                .ForeignKey("dbo.AssociationRule", t => t.AssociationRuleId, cascadeDelete: true)
                .ForeignKey("dbo.DimensionValue", t => t.DimensionValueId, cascadeDelete: true)
                .Index(t => t.AssociationRuleId)
                .Index(t => t.DimensionValueId);
            
            CreateTable(
                "dbo.ConditionDimensionValues",
                c => new
                    {
                        AssociationRuleId = c.Int(nullable: false),
                        DimensionValueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AssociationRuleId, t.DimensionValueId })
                .ForeignKey("dbo.AssociationRule", t => t.AssociationRuleId, cascadeDelete: true)
                .ForeignKey("dbo.DimensionValue", t => t.DimensionValueId, cascadeDelete: true)
                .Index(t => t.AssociationRuleId)
                .Index(t => t.DimensionValueId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssociationRule", "SuccedentMeasure_Id", "dbo.Measure");
            DropForeignKey("dbo.ConditionDimensionValues", "DimensionValueId", "dbo.DimensionValue");
            DropForeignKey("dbo.ConditionDimensionValues", "AssociationRuleId", "dbo.AssociationRule");
            DropForeignKey("dbo.AntecedentDimensionValues", "DimensionValueId", "dbo.DimensionValue");
            DropForeignKey("dbo.AntecedentDimensionValues", "AssociationRuleId", "dbo.AssociationRule");
            DropForeignKey("dbo.Dimension", "ParentDimension_Id", "dbo.Dimension");
            DropForeignKey("dbo.DimensionValue", "Dimension_Id", "dbo.Dimension");
            DropForeignKey("dbo.MiningTask", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.AssociationRule", "MiningTask_Id", "dbo.MiningTask");
            DropForeignKey("dbo.Measure", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Dimension", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Attribute", "DataSet_Id", "dbo.Dataset");
            DropIndex("dbo.ConditionDimensionValues", new[] { "DimensionValueId" });
            DropIndex("dbo.ConditionDimensionValues", new[] { "AssociationRuleId" });
            DropIndex("dbo.AntecedentDimensionValues", new[] { "DimensionValueId" });
            DropIndex("dbo.AntecedentDimensionValues", new[] { "AssociationRuleId" });
            DropIndex("dbo.MiningTask", new[] { "DataSet_Id" });
            DropIndex("dbo.Measure", new[] { "DataSet_Id" });
            DropIndex("dbo.Attribute", new[] { "DataSet_Id" });
            DropIndex("dbo.Dimension", new[] { "ParentDimension_Id" });
            DropIndex("dbo.Dimension", new[] { "DataSet_Id" });
            DropIndex("dbo.DimensionValue", new[] { "Dimension_Id" });
            DropIndex("dbo.AssociationRule", new[] { "SuccedentMeasure_Id" });
            DropIndex("dbo.AssociationRule", new[] { "MiningTask_Id" });
            DropTable("dbo.ConditionDimensionValues");
            DropTable("dbo.AntecedentDimensionValues");
            DropTable("dbo.MiningTask");
            DropTable("dbo.Measure");
            DropTable("dbo.Attribute");
            DropTable("dbo.Dataset");
            DropTable("dbo.Dimension");
            DropTable("dbo.DimensionValue");
            DropTable("dbo.AssociationRule");
        }
    }
}
