using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.DataAccess
{
    public class DbInitializer : System.Data.Entity.CreateDatabaseIfNotExists<DbContext>
    {
        protected override void Seed(DbContext context)
        {
            context.SaveChanges();
        }
    }
}