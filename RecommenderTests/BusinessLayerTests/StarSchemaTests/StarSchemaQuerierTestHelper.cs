using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender2.Business.DTO;
using Recommender2.Business.Enums;
using Recommender2.DataAccess;

namespace RecommenderTests.BusinessLayerTests.StarSchemaTests
{
    public static class StarSchemaQuerierTestHelper
    {
        public static DataTable CreateSampleDimensionDataTable(DataType dataType)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            if (dataType == DataType.DateTime)
            {
                table.Columns.Add("Value", typeof(DateTime));
                table.Rows.Add(1, new DateTime(2016, 12, 24, 10, 59, 59));
                table.Rows.Add(2, DateTime.Now);
                table.Rows.Add(3, DateTime.UtcNow);
            }
            else
            {
                table.Columns.Add("Value", typeof(string));
                table.Rows.Add(1, "Bread");
                table.Rows.Add(2, "Bun");
                table.Rows.Add(3, "Cake");
            }
            return table;
        }

        public static DataTable CreateSampleFactDataTable(string measureName, List<double> values)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add(measureName, typeof(double));
            for (int i = 0; i < values.Count; i++)
            {
                table.Rows.Add(i, values[i]);
            }
            return table;
        }

        public static FlatDimensionDto CreateFilter(string datasetName, string name, int id, List<DimensionValue> dimensionValues)
        {
            return new FlatDimensionDto
            {
                DatasetName = datasetName,
                Name = name,
                Id = id,
                DimensionValues = dimensionValues
            };
        }

        public static Column CreateColumnFilter(string name, string value)
        {
            return new Column
            {
                Name = name,
                Value = value
            };
        }
    }
}
