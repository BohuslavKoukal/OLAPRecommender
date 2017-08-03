using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;
using Constants = Recommender.Common.Constants.Constants;

namespace Recommender.Business.StarSchema
{
    public interface IStarSchemaBuilder
    {
        void CreateAndFillDimensionTables(string prefix, string datasetName, List<Dimension> dimensions, DataTable values);
        void CreateFactTable(Dataset dataset, List<Dimension> dimensions, List<Measure> measures);
        void FillFactTable(string prefix, string datasetName, List<Dimension> dimensions, List<Measure> measures, DataTable values);
        void CreateView(Dataset dataset, List<Dimension> dimensions, List<Measure> measures);
        void DropAllTables(string prefix, string datasetName, List<Dimension> dimensions);
    }

    public class StarSchemaBuilder : StarSchemaBase, IStarSchemaBuilder
    {

        public StarSchemaBuilder(IQueryBuilder queryBuilder, IDataAccessLayer data) 
            : base(queryBuilder, data)
        {
        }

        #region public

        public void CreateAndFillDimensionTables(string prefix, string datasetName, List<Dimension> dimensions, DataTable values)
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
                        CreateDimensionTable(prefix, datasetName, dimension.Name, dimension.Type.ToType().ToSqlType(), childDimensions.Select(d => d.Name).ToList());
                        FillDimensionTable(prefix, datasetName, dimension, values, childDimensions);
                        existingDimensions.Add(dimension.Id);
                    }
                }
            }
        }

        public void CreateFactTable(Dataset dataset, List<Dimension> dimensions, List<Measure> measures)
        {
            var rootDimensionNames = dimensions.Where(d => d.ParentDimension == null).Select(d => d.Name).ToList<string>();
            var measuresColumns = measures.Select(m => new Column { Name = m.Name, Type = m.Type.ToType().ToSqlType() });
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(rootDimensionNames.Select(dimensionName => new ForeignKey
            {
                KeyName = dimensionName + Constants.String.Id,
                Reference = dataset.GetPrefix() + dataset.Name + dimensionName
            }));
            QueryBuilder.CreateTable(dataset.GetFactTableName(), measuresColumns, foreignKeys);
        }

        public void CreateView(Dataset dataset, List<Dimension> dimensions, List<Measure> measures)
        {
            QueryBuilder.CreateView(dataset.GetPrefix() + dataset.Name, dataset.GetFactTableName(), OrderDimensionsTopDown(dimensions), measures);
        }       

        public void FillFactTable(string prefix, string datasetName, List<Dimension> dimensions, List<Measure> measures, DataTable values)
        {
            var queryCache = new QueryCache();
            var allRows = new List<List<Column>>();
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
                    Name = dimension.Name + Constants.String.Id,
                    Value = GetDimensionId(prefix + datasetName + dimension.Name, row[dimension.Name].ToString(dimension.Type.ToType()), queryCache).ToString()
                }).ToList();
                allRows.Add(dimensionColumns.Concat(measureColumns).ToList());
            }
            QueryBuilder.Insert(prefix + datasetName + Constants.String.FactTable, allRows);
        }

        public void DropAllTables(string prefix, string datasetName, List<Dimension> dimensions)
        {
            foreach (var dimension in dimensions)
            {
                QueryBuilder.Drop(prefix + datasetName + dimension.Name, "TABLE");
            }
            QueryBuilder.Drop(prefix + datasetName + Constants.String.FactTable, "TABLE");
            QueryBuilder.Drop(prefix + datasetName + Constants.String.View, "VIEW");
        }

        #endregion

        #region private

        private List<Dimension> OrderDimensionsTopDown(List<Dimension> dimensions)
        {
            var orderedDimensions = new List<Dimension>();
            while (orderedDimensions.Count != dimensions.Count)
            {
                foreach (var dimension in dimensions)
                {
                    // Add dimension if its parent is added
                    // If this dimension is not added yet
                    if (!orderedDimensions.Select(d => d.Id).Contains(dimension.Id))
                    {
                        if (dimension.ParentDimension == null)
                        {
                            orderedDimensions.Add(dimension);
                        }
                        else
                        {
                            // Add dimension only if its parent is already added
                            if (orderedDimensions.Select(d => d.Id).Contains(dimension.ParentDimension.Id))
                                orderedDimensions.Add(dimension);
                        }
                    }
                }
            }
            return orderedDimensions;
        }

        private int GetDimensionId(string tableName, string value, QueryCache cache)
        {
            var ret = cache.GetId(tableName, value);
            if (ret < 1)
            {
                ret = GetDimensionId(tableName, value);
                cache.Add(tableName, value, ret);
            }
            return ret;
        }

        private int GetDimensionId(string tableName, string value)
        {
                return Convert.ToInt32
                (QueryBuilder.Select(tableName, new Column { Name = Constants.String.Value, Value = value }).Rows.Cast<DataRow>().First()[0]);
        }

        private void FillDimensionTable(string prefix, string datasetName, Dimension dimension, DataTable values, List<Dimension> children)
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
                    Value = GetDimensionId(prefix + datasetName + child.Name, row[child.Name].ToString()).ToString()
                }));
                allRowsToInsert.Add(dimensionColumns);
            }
            QueryBuilder.Insert(prefix + datasetName + dimension.Name, allRowsToInsert);
        }

        private void CreateDimensionTable(string prefix, string datasetName, string dimensionName, string datatype, List<string> childrenNames)
        {
            var tableName = prefix + datasetName + dimensionName;
            var columns = new List<Column> { new Column { Name = Constants.String.Value, Type = datatype } };
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(childrenNames.Select(childName => new ForeignKey
            {
                KeyName = childName + Constants.String.Id,
                Reference = prefix + datasetName + childName
            }));
            QueryBuilder.CreateTable(tableName, columns, foreignKeys);
        }

        #endregion
    }
}