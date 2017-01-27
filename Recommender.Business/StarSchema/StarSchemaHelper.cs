using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.StarSchema
{
    public static class StarSchemaHelper
    {
        public static List<DimensionValue> GetDimensionValues(Dimension dimension, DataTable values)
        {
            var distinctValues = values.GetDistinctTable(dimension.Type.ToType(), dimension.Name);
            return (from DataRow row in distinctValues.Rows
                select new DimensionValue
                {
                    Dimension = dimension, Value = row[dimension.Name].ToString(dimension.Type.ToType())
                }).ToList();
        }
    }
}
