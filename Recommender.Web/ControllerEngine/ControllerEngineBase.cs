using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;

namespace Recommender.Web.ControllerEngine
{
    public class ControllerEngineBase
    {
        protected readonly IDataAccessLayer Data;
        protected readonly IStarSchemaQuerier StarSchemaQuerier;

        public ControllerEngineBase(IDataAccessLayer data, IStarSchemaQuerier starSchemaQuerier)
        {
            Data = data;
            StarSchemaQuerier = starSchemaQuerier;
        }
    }
}
