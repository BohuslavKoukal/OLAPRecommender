using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Recommender2;
using Recommender2.Business;
using Recommender2.Business.Helpers;
using Recommender2.Business.Service;
using Recommender2.DataAccess;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace Recommender2
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IDataAccessLayer>().To<DataAccessLayer>();
            kernel.Bind<IDatasetViewModelMapper>().To<DatasetViewModelMapper>();
            kernel.Bind<IBrowseCubeViewModelMapper>().To<BrowseCubeViewModelMapper>();
            kernel.Bind<IStarSchemaQuerier>().To<StarSchemaQuerier>();
            kernel.Bind<IGraphService>().To<GraphService>();
            kernel.Bind<IDimensionTreeBuilder>().To<DimensionTreeBuilder>();
            kernel.Bind<IQueryBuilder>().To<QueryBuilder>();
            kernel.Bind<IDbConnection>().To<DbConnection>();
            kernel.Bind<IConfiguration>().To<Configuration>();
        }        
    }
}
