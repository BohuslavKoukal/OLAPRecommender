using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.DataAccess;

namespace Recommender2.Business
{
    public class StarSchemaBase 
    {
        protected readonly QueryBuilder QueryBuilder;
        protected readonly DbConnection DbConnection;
        protected readonly DataAccessLayer Data;

        public StarSchemaBase(QueryBuilder queryBuilder, DataAccessLayer data, DbConnection dbConnection)
        {
            QueryBuilder = queryBuilder;
            DbConnection = dbConnection;
            Data = data;
        }


    }
}