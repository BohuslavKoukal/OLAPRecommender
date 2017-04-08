using Ninject.Modules;
using Recommender.Business;
using Recommender.Business.FileHandling;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Common;
using Recommender.Data.DataAccess;
using Recommender.Web.ViewModels.Mappers;

namespace Recommender.Web
{
    public class WebModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatasetViewModelMapper>().To<DatasetViewModelMapper>();
            Bind<IBrowseCubeViewModelMapper>().To<BrowseCubeViewModelMapper>();
            Bind<IMiningTaskViewModelMapper>().To<MiningTaskViewModelMapper>();
            Bind<IConfiguration>().To<Configuration>();
        }
    }

    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataAccessLayer>().To<DataAccessLayer>();
            Bind<IQueryBuilder>().To<QueryBuilder>();
            Bind<IDbConnection>().To<DbConnection>();
        }
    }

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
