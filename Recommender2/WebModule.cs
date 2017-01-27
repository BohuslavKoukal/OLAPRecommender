using Ninject.Modules;
using Recommender.Common;
using Recommender2.ViewModels.Mappers;

namespace Recommender2
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
}
