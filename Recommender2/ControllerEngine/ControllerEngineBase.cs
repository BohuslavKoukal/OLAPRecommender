using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business;

namespace Recommender2.ControllerEngine
{
    public class ControllerEngineBase
    {
        protected readonly IDataDecorator Data;
        protected readonly IStarSchemaQuerier StarSchemaQuerier;

        public ControllerEngineBase(IDataDecorator data, IStarSchemaQuerier starSchemaQuerier)
        {
            Data = data;
            StarSchemaQuerier = starSchemaQuerier;
        }


    }
}
