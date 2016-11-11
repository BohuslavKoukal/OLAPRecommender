using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Recommender2.Business.DTO;
using Recommender2.Business.Enums;
using Recommender2.DataAccess;
using Recommender2.Models;

namespace Recommender2.Business
{
    public class StarSchemaQuerier : StarSchemaBase
    {
        public StarSchemaQuerier(QueryBuilder queryBuilder, DataAccessLayer data, DbConnection dbConnection) 
            : base(queryBuilder, data, dbConnection)
        {
        }

        public List<DimensionValue> GetValuesOfDimension(Dimension dimension, Column selector = null)
        {
            DataTable values;
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                values = selector == null ? QueryBuilder.Select(dimension.TableName, new List<Column>(),  conn) 
                    : QueryBuilder.Select(dimension.TableName, selector, conn);
                conn.Close();
            }
            if(dimension.Type == (int) DataType.DateTime)
                return (from DataRow row
                    in values.Rows
                        select new DimensionValue { Id = Convert.ToInt32(row["Id"]), Value = ((DateTime) row["Value"]).ToString("d"), Dimension = dimension })
                    .ToList();
            return (from DataRow row 
                    in values.Rows
                    select new DimensionValue { Id = Convert.ToInt32(row["Id"]), Value = row["Value"].ToString(), Dimension = dimension })
                    .ToList();
        }

        public List<double> GetValuesFromFactTable(string datasetName, Column dimensionSelector, string measureName)
        {
            return GetValuesFromFactTable(datasetName, new List<Column> {dimensionSelector}, measureName);
        }

        public List<double> GetValuesFromFactTable(string datasetName, List<Column> dimensionSelectors, string measureName)
        {
            DataTable values;
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                values = QueryBuilder.Select(datasetName + "FactTable", dimensionSelectors, conn);
                conn.Close();
            }
            return (from DataRow row
                    in values.Rows
                    select Convert.ToDouble(row[measureName]))
                    .ToList();
        }

        public double GetFactTableSum(DimensionValue dimensionValue, Measure measure)
        {
            return GetFactTableSum(new List<DimensionValue> {dimensionValue}, measure);
        }

        public double GetFactTableSum(List<DimensionValue> dimensionValues, Measure measure)
        {
            DataTable queryResult;
            var factTableName = dimensionValues.First().Dimension.FactTableName;
            var dimensionIds = new List<DimensionIds>();
            foreach (var value in dimensionValues)
            {
                dimensionIds.Add(GetRootDimensionIds(value.Dimension, value.Id));
            }
            var selectors = new List<List<Column>>();
            foreach (var dimensionId in dimensionIds)
            {
                var orList = new List<Column>();
                orList.AddRange(dimensionId.Ids.Select(dId => new Column { Name = dimensionId.Dimension.IdName, Value = dId.ToString() }));
                selectors.Add(orList);
            }
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                queryResult = QueryBuilder.Select(factTableName, selectors, conn);
                conn.Close();
            }
            
            return AggregateData(queryResult, measure.Name);
        }

        private double AggregateData(DataTable table, string columnName)
        {
            return table.Rows.Cast<DataRow>().Sum(row => Convert.ToDouble(row[columnName]));
        }

        // Returns root dimension and list of its ids for concrete non-root dimension
        private DimensionIds GetRootDimensionIds(Dimension dimension, int id)
        {
            bool isRoot = dimension.ParentDimension == null;
            var childDimension = dimension;
            var oldIds = new List<int> { id };
            var newIds = new List<int> { id };
            while (!isRoot)
            {
                var parentDimension = childDimension.ParentDimension;
                DataTable currentIds;
                using (var conn = DbConnection.Connection)
                {
                    conn.Open();
                    var selectors = new List<List<Column>>
                    {
                        oldIds.Select(childId => new Column {Name = childDimension.IdName, Value = childId.ToString()})
                            .ToList()
                    };
                    oldIds = newIds;
                    currentIds = QueryBuilder.Select(parentDimension.TableName, selectors, conn);
                    conn.Close();
                }
                newIds.Clear();
                newIds.AddRange(from DataRow currentId in currentIds.Rows select Convert.ToInt32(currentId["Id"]));
                childDimension = parentDimension;
                parentDimension = parentDimension.ParentDimension;
                isRoot = parentDimension == null;
            }
            return new DimensionIds
            {
                Dimension = childDimension,
                Ids = newIds
            };
        }
    }
}