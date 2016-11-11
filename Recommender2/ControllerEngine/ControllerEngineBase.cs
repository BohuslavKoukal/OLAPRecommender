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
        protected readonly DataAccessLayer Data;
        protected readonly CsvHandler CsvHandler;
        protected readonly StarSchemaBuilder StarSchemaBuilder;
        protected readonly StarSchemaQuerier StarSchemaQuerier;

        public ControllerEngineBase(DataAccessLayer data, CsvHandler csvHandler,
            StarSchemaBuilder starSchemaBuilder, StarSchemaQuerier starSchemaQuerier)
        {
            Data = data;
            CsvHandler = csvHandler;
            StarSchemaBuilder = starSchemaBuilder;
            StarSchemaQuerier = starSchemaQuerier;
        }


    }
}
