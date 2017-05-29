using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using Microsoft.AspNet.Identity.EntityFramework;
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
        DbSet<Succedent> Succedents { get; set; }
        DbSet<DimensionValue> DimensionValues { get; set; }
        int SaveChanges();
    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class DbContext : IdentityDbContext<ApplicationUser>, IDbContext
    {
        public DbContext() : base("DbContext")
        {
            var adapter = (IObjectContextAdapter)this;
            var objectContext = adapter.ObjectContext;
            objectContext.CommandTimeout = 2 * 60;
        }

        public static DbContext Create()
        {
            return new DbContext();
        }

        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<MiningTask> MiningTasks { get; set; }
        public DbSet<AssociationRule> AssociationRules { get; set; }
        public DbSet<Succedent> Succedents { get; set; }
        public DbSet<DimensionValue> DimensionValues { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
            modelBuilder.Entity<MiningTask>()
                .HasMany(mt => mt.ConditionDimensions)
                .WithMany(cd => cd.Tasks)
                .Map(x =>
                {
                    x.MapLeftKey("MiningTaskId");
                    x.MapRightKey("ConditionDimensionId");
                    x.ToTable("MiningTaskConditionDimensions");
                });
        }
    }
}