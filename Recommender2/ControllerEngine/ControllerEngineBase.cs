using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
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
