using Recommender.Data.DataAccess;

namespace Recommender.Business
{
    public class StarSchemaBase 
    {
        protected readonly IQueryBuilder QueryBuilder;
        protected readonly IDataDecorator Data;

        public StarSchemaBase(IQueryBuilder queryBuilder, IDataDecorator data)
        {
            QueryBuilder = queryBuilder;
            Data = data;
        }


    }
}