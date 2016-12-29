namespace Recommender.Data.DataAccess
{
    public class DbInitializer : System.Data.Entity.CreateDatabaseIfNotExists<DbContext>
    {
        protected override void Seed(DbContext context)
        {
            context.SaveChanges();
        }
    }
}