using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Recommender2.Business.Enums;
using Recommender2.Business.Helpers;
using Recommender2.DataAccess;
using Recommender2.Models;

namespace Recommender2.Business
{
    
    public class StarSchemaBuilder : StarSchemaBase
    {

        public StarSchemaBuilder(QueryBuilder queryBuilder, DataAccessLayer data, DbConnection dbConnection) 
            : base(queryBuilder, data, dbConnection)
        {
        }

        #region public

        public void CreateAndFillDimensionTables(string datasetName, List<Dimension> dimensions, DataTable values)
        {
            var existingDimensions = new List<int>();
            while (existingDimensions.Count != dimensions.Count)
            {
                foreach (var dimension in dimensions)
                {
                    var childDimensions = Data.GetChildDimensions(dimension.Id);
                    // Create dimension if all child dimensions exist and current dimension does not
                    if (!childDimensions.Select(d => d.Id).Except(existingDimensions).Any() &&
                        !existingDimensions.Contains(dimension.Id))
                    {
                        CreateDimensionTable(datasetName, dimension.Name, ((DataType)dimension.Type).ToSqlType(), childDimensions.Select(d => d.Name).ToList());
                        using (var conn = DbConnection.Connection)
                        {
                            conn.Open();
                            FillDimensionTable(datasetName, dimension, values, childDimensions, conn);
                            conn.Close();
                        }
                        existingDimensions.Add(dimension.Id);
                    }
                }
            }
        }

        public void CreateFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures)
        {
            var rootDimensionNames = dimensions.Where(d => d.ParentDimension == null).Select(d => d.Name).ToList();
            var measuresColumns = measures.Select(m => new Column {Name = m.Name, Type = ((DataType)m.Type).ToSqlType() });
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(rootDimensionNames.Select(dimensionName => new ForeignKey
            {
                KeyName = dimensionName + "Id",
                Reference = datasetName + dimensionName
            }));
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                QueryBuilder.CreateTable(datasetName + "FactTable", measuresColumns, foreignKeys, conn);
                conn.Close();
            }
        }        

        public void FillFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures, DataTable values)
        {
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                foreach (DataRow row in values.Rows)
                {
                    var measureColumns = measures.Select(measure => new Column
                    {
                        Name = measure.Name,
                        Value = row[measure.Name].ToString()
                    }).ToList();
                    // We insert fks only for root dimensions
                    var dimensionColumns = dimensions.Where(d => d.ParentDimension == null).Select(dimension => new Column
                    {
                        Name = dimension.Name + "Id",
                        Value = GetDimensionId(datasetName + dimension.Name, row[dimension.Name].ToString(dimension.Type)).ToString()
                    }).ToList();
                
                    QueryBuilder.Insert(datasetName + "FactTable", dimensionColumns.Concat(measureColumns), conn);
                }
                conn.Close();
            }
        }

        #endregion

        #region private

        private int GetDimensionId(string tableName, string value)
        {
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                return Convert.ToInt32
                    (QueryBuilder.Select(tableName, new Column { Name = "Value", Value = value }, conn).Rows.Cast<DataRow>().First()[0]);
            }
            
        }

        private void FillDimensionTable(string datasetName, Dimension dimension, DataTable values, List<Dimension> children, MySqlConnection conn)
        {
            var distinctValues = values.GetDistinctTable(dimension.Type, dimension.Name);
            foreach (DataRow row in distinctValues.Rows)
            {
                var dimensionColumns = new List<Column>
                {
                    new Column { Name = "Value", Value = row[dimension.Name].ToString(dimension.Type) }
                };
                dimensionColumns.AddRange(children.Select(child => new Column
                {
                    Name = child.Name + "Id",
                    Value = GetDimensionId(datasetName + child.Name, row[child.Name].ToString(child.Type)).ToString()
                }));
                QueryBuilder.Insert(datasetName + dimension.Name, dimensionColumns, conn);
            }
        }

        private void CreateDimensionTable(string datasetName, string dimensionName, string datatype, List<string> childrenNames)
        {
            var tableName = datasetName + dimensionName;
            var columns = new List<Column> { new Column { Name = "Value", Type = datatype } };
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(childrenNames.Select(childName => new ForeignKey
            {
                KeyName = childName + "Id",
                Reference = datasetName + childName
            }));
            using (var conn = DbConnection.Connection)
            {
                conn.Open();
                QueryBuilder.CreateTable(tableName, columns, foreignKeys, conn);
                conn.Close();
            }
        }

        #endregion
    }
}