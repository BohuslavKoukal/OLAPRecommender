using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender2.Business;
using Recommender2.DataAccess;

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
