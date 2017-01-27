using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Recommender.Business;
using Recommender.Business.FileHandling;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;

namespace Recommender.DependencyResolver
{
    public class BusinessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStarSchemaQuerier>().To<StarSchemaQuerier>();
            Bind<IGraphService>().To<GraphService>();
            Bind<IDimensionTreeBuilder>().To<DimensionTreeBuilder>();
            Bind<IFileHandler>().To<FileHandler>();
        }
    }
}
