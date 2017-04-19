using Ninject.Modules;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.FileHandling;
using Recommender.Business.FileHandling.Csv;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Common;
using Recommender.Data.DataAccess;
using Recommender.Web.Validations;
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
            Bind<IAttributeViewModelMapper>().To<AttributeViewModelMapper>();
            Bind<IAssociationRuleToViewMapper>().To<AssociationRuleToViewMapper>();
            Bind<IConfiguration>().To<Configuration>();
            Bind<IInputValidations>().To<InputValidations>();
        }
    }

    public class DataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataAccessLayer>().To<DataAccessLayer>();
            Bind<IQueryBuilder>().To<QueryBuilder>();
            Bind<IDbConnection>().To<DbConnection>();
            Bind<IDbContext>().To<DbContext>();
        }
    }

    public class BusinessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IStarSchemaQuerier>().To<StarSchemaQuerier>();
            Bind<IStarSchemaBuilder>().To<StarSchemaBuilder>();
            Bind<IGraphService>().To<GraphService>();
            Bind<IDimensionTreeBuilder>().To<DimensionTreeBuilder>();
            Bind<IFileHandler>().To<FileHandler>();
            Bind<ICsvHandler>().To<CsvHandler>();
            Bind<IAssociationRulesTaskProcessor>().To<AssociationRulesTaskProcessor>();
            Bind<IDataDiscretizator>().To<DataDiscretizator>();
        }
    }
}
