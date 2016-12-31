using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Recommender.Business;
using Recommender.Business.Service;
using Recommender.Data.DataAccess;
using DbConnection = System.Data.Common.DbConnection;
using IDbConnection = System.Data.IDbConnection;

namespace Recommender.DependencyResolver
{
    public class BusinessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStarSchemaQuerier>().To<StarSchemaQuerier>();
            Bind<IGraphService>().To<GraphService>();
            Bind<IDimensionTreeBuilder>().To<DimensionTreeBuilder>();
        }
    }
}
