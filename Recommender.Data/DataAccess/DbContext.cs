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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}