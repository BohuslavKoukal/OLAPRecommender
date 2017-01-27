using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Recommender.Data.Models;

namespace Recommender.Data.DataAccess
{
    public interface IDbContext
    {
        DbSet<Attribute> Attributes { get; set; }
        DbSet<Dataset> Datasets { get; set; }
        DbSet<Dimension> Dimensions { get; set; }
        DbSet<Measure> Measures { get; set; }
        DbSet<MiningTask> MiningTasks { get; set; }
        DbSet<AssociationRule> AssociationRules { get; set; }
        //DbSet<LiteralConjunction> LiteralConjunctions { get; set; }
        DbSet<DimensionValue> DimensionValues { get; set; }
    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class DbContext : System.Data.Entity.DbContext, IDbContext
    {
        public DbContext() : base("DbContext")
        {
        }

        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<MiningTask> MiningTasks { get; set; }
        public DbSet<AssociationRule> AssociationRules { get; set; }
        public DbSet<DimensionValue> DimensionValues { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<AssociationRule>()
                .HasMany(ar => ar.AntecedentValues)
                .WithMany(av => av.AntecedentRules)
                .Map(x =>
                {
                    x.MapLeftKey("AssociationRuleId");
                    x.MapRightKey("DimensionValueId");
                    x.ToTable("AntecedentDimensionValues");
                });
            modelBuilder.Entity<AssociationRule>()
                .HasMany(ar => ar.ConditionValues)
                .WithMany(av => av.ConditionRules)
                .Map(x =>
                {
                    x.MapLeftKey("AssociationRuleId");
                    x.MapRightKey("DimensionValueId");
                    x.ToTable("ConditionDimensionValues");
                });
        }
    }
}