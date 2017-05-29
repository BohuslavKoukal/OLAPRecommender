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
                        Text = c.String(unicode: false),
                        MiningTask_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MiningTask", t => t.MiningTask_Id)
                .Index(t => t.MiningTask_Id);
            
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
                        KeepFilePrivate = c.Boolean(nullable: false),
                        Preprocessed = c.Boolean(nullable: false),
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
                "dbo.Succedent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SuccedentText = c.String(unicode: false),
                        Aad = c.Double(nullable: false),
                        Base = c.Double(nullable: false),
                        AssociationRule_Id = c.Int(nullable: false),
                        Measure_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssociationRule", t => t.AssociationRule_Id, cascadeDelete: true)
                .ForeignKey("dbo.Measure", t => t.Measure_Id)
                .Index(t => t.AssociationRule_Id)
                .Index(t => t.Measure_Id);
            
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
                        ConditionRequired = c.Boolean(nullable: false),
                        TaskStartTime = c.DateTime(nullable: false, precision: 0),
                        TaskDuration = c.Time(nullable: false, precision: 0),
                        FailedReason = c.String(unicode: false),
                        DataSet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dataset", t => t.DataSet_Id)
                .Index(t => t.DataSet_Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Name = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        RoleId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Email = c.String(maxLength: 256, storeType: "nvarchar"),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(unicode: false),
                        SecurityStamp = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(precision: 0),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ClaimType = c.String(unicode: false),
                        ClaimValue = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ProviderKey = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MiningTaskConditionDimensions",
                c => new
                    {
                        MiningTaskId = c.Int(nullable: false),
                        ConditionDimensionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MiningTaskId, t.ConditionDimensionId })
                .ForeignKey("dbo.MiningTask", t => t.MiningTaskId, cascadeDelete: true)
                .ForeignKey("dbo.Dimension", t => t.ConditionDimensionId, cascadeDelete: true)
                .Index(t => t.MiningTaskId)
                .Index(t => t.ConditionDimensionId);
            
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
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ConditionDimensionValues", "DimensionValueId", "dbo.DimensionValue");
            DropForeignKey("dbo.ConditionDimensionValues", "AssociationRuleId", "dbo.AssociationRule");
            DropForeignKey("dbo.AntecedentDimensionValues", "DimensionValueId", "dbo.DimensionValue");
            DropForeignKey("dbo.AntecedentDimensionValues", "AssociationRuleId", "dbo.AssociationRule");
            DropForeignKey("dbo.Dimension", "ParentDimension_Id", "dbo.Dimension");
            DropForeignKey("dbo.DimensionValue", "Dimension_Id", "dbo.Dimension");
            DropForeignKey("dbo.MiningTask", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.MiningTaskConditionDimensions", "ConditionDimensionId", "dbo.Dimension");
            DropForeignKey("dbo.MiningTaskConditionDimensions", "MiningTaskId", "dbo.MiningTask");
            DropForeignKey("dbo.AssociationRule", "MiningTask_Id", "dbo.MiningTask");
            DropForeignKey("dbo.Succedent", "Measure_Id", "dbo.Measure");
            DropForeignKey("dbo.Succedent", "AssociationRule_Id", "dbo.AssociationRule");
            DropForeignKey("dbo.Measure", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Dimension", "DataSet_Id", "dbo.Dataset");
            DropForeignKey("dbo.Attribute", "DataSet_Id", "dbo.Dataset");
            DropIndex("dbo.ConditionDimensionValues", new[] { "DimensionValueId" });
            DropIndex("dbo.ConditionDimensionValues", new[] { "AssociationRuleId" });
            DropIndex("dbo.AntecedentDimensionValues", new[] { "DimensionValueId" });
            DropIndex("dbo.AntecedentDimensionValues", new[] { "AssociationRuleId" });
            DropIndex("dbo.MiningTaskConditionDimensions", new[] { "ConditionDimensionId" });
            DropIndex("dbo.MiningTaskConditionDimensions", new[] { "MiningTaskId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.MiningTask", new[] { "DataSet_Id" });
            DropIndex("dbo.Succedent", new[] { "Measure_Id" });
            DropIndex("dbo.Succedent", new[] { "AssociationRule_Id" });
            DropIndex("dbo.Measure", new[] { "DataSet_Id" });
            DropIndex("dbo.Attribute", new[] { "DataSet_Id" });
            DropIndex("dbo.Dimension", new[] { "ParentDimension_Id" });
            DropIndex("dbo.Dimension", new[] { "DataSet_Id" });
            DropIndex("dbo.DimensionValue", new[] { "Dimension_Id" });
            DropIndex("dbo.AssociationRule", new[] { "MiningTask_Id" });
            DropTable("dbo.ConditionDimensionValues");
            DropTable("dbo.AntecedentDimensionValues");
            DropTable("dbo.MiningTaskConditionDimensions");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.MiningTask");
            DropTable("dbo.Succedent");
            DropTable("dbo.Measure");
            DropTable("dbo.Attribute");
            DropTable("dbo.Dataset");
            DropTable("dbo.Dimension");
            DropTable("dbo.DimensionValue");
            DropTable("dbo.AssociationRule");
        }
    }
}
