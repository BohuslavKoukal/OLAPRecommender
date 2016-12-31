using Recommender.Data.DataAccess;

namespace Recommender.Business
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