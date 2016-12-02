using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.DataAccess;

namespace Recommender2.Business
{
    public class StarSchemaBase 
    {
        protected readonly IQueryBuilder QueryBuilder;
        protected readonly IDataAccessLayer Data;

        public StarSchemaBase(IQueryBuilder queryBuilder, IDataAccessLayer data)
        {
            QueryBuilder = queryBuilder;
            Data = data;
        }


    }
}