using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Constants = Recommender.Common.Constants.Constants;

namespace Recommender.Business.StarSchema
{
    public interface IStarSchemaBuilder
    {
        void CreateAndFillDimensionTables(string datasetName, List<Dimension> dimensions, DataTable values);
        void CreateFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures);
        void FillFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures, DataTable values);
    }

    public class StarSchemaBuilder : StarSchemaBase, IStarSchemaBuilder
    {

        public StarSchemaBuilder(QueryBuilder queryBuilder, IDataAccessLayer data) 
            : base(queryBuilder, data)
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
                        CreateDimensionTable(datasetName, dimension.Name, dimension.Type.ToType().ToSqlType(), childDimensions.Select(d => d.Name).ToList());
                        FillDimensionTable(datasetName, dimension, values, childDimensions);
                        existingDimensions.Add(dimension.Id);
                    }
                }
            }
        }

        public void CreateFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures)
        {
            var rootDimensionNames = dimensions.Where(d => d.ParentDimension == null).Select(d => d.Name).ToList<string>();
            var measuresColumns = measures.Select(m => new Column {Name = m.Name, Type = m.Type.ToType().ToSqlType() });
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(rootDimensionNames.Select(dimensionName => new ForeignKey
            {
                KeyName = dimensionName + Constants.String.Id,
                Reference = datasetName + dimensionName
            }));
            QueryBuilder.CreateTable(datasetName + Constants.String.FactTable, measuresColumns, foreignKeys);
        }        

        public void FillFactTable(string datasetName, List<Dimension> dimensions, List<Measure> measures, DataTable values)
        {
            foreach (DataRow row in values.Rows)
            {
                var measureColumns = measures.Select(measure => new Column
                {
                    Name = measure.Name,
                    Value = row[(string) measure.Name].ToString()
                }).ToList();
                // We insert fks only for root dimensions
                var dimensionColumns = dimensions.Where(d => d.ParentDimension == null).Select(dimension => new Column
                {
                    Name = dimension.Name + Constants.String.Id,
                    Value = GetDimensionId(datasetName + dimension.Name, row[dimension.Name].ToString(dimension.Type.ToType())).ToString()
                }).ToList();
                
                QueryBuilder.Insert(datasetName + Constants.String.FactTable, dimensionColumns.Concat(measureColumns));
            }
        }

        #endregion

        #region private

        private int GetDimensionId(string tableName, string value)
        {
            return Convert.ToInt32
                (QueryBuilder.Select(tableName, new Column { Name = Constants.String.Value, Value = value }).Rows.Cast<DataRow>().First()[0]);
            
        }

        private void FillDimensionTable(string datasetName, Dimension dimension, DataTable values, List<Dimension> children)
        {
            var distinctValues = values.GetDistinctTable(dimension.Type.ToType(), dimension.Name);
            var allRowsToInsert = new List<List<Column>>();
            foreach (DataRow row in distinctValues.Rows)
            {
                var dimensionColumns = new List<Column>
                {
                    new Column
                    {
                        Name = Constants.String.Value,
                        Value = row[dimension.Name].ToString(dimension.Type.ToType())
                    }
                };
                dimensionColumns.AddRange(children.Select(child => new Column
                {
                    Name = child.Name + Constants.String.Id,
                    Value = GetDimensionId(datasetName + child.Name, row[child.Name].ToString()).ToString()
                }));
                allRowsToInsert.Add(dimensionColumns);
            }
            QueryBuilder.Insert(datasetName + dimension.Name, allRowsToInsert);
        }

        private void CreateDimensionTable(string datasetName, string dimensionName, string datatype, List<string> childrenNames)
        {
            var tableName = datasetName + dimensionName;
            var columns = new List<Column> { new Column { Name = Constants.String.Value, Type = datatype } };
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(childrenNames.Select(childName => new ForeignKey
            {
                KeyName = childName + Constants.String.Id,
                Reference = datasetName + childName
            }));
            QueryBuilder.CreateTable(tableName, columns, foreignKeys);
        }

        #endregion
    }
}