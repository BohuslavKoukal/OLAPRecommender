using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Recommender.Data.DataAccess;

namespace Recommender.DependencyResolver
{
    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataAccessLayer>().To<DataAccessLayer>();
            Bind<IQueryBuilder>().To<QueryBuilder>();
            Bind<IDbConnection>().To<DbConnection>();
        }
    }
}
