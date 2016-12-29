using System.Data.Entity.Migrations;

namespace Recommender.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DataAccess.DbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DataAccess.DbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //You can use the DbSet<T>.AddOrUpdate() helper extension method
            //to avoid creating duplicate seed data.E.g.

              //context.Datasets.AddOrUpdate(
              //  new Dataset { Name = "Dataset1" }
              //);

        }
    }
}
