using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business
{
    public interface IStarSchemaBuilder
    {
        void CreateAndFillDimensionTables(string datasetName, List<DimensionDto> dimensions, DataTable values);
        void CreateFactTable(string datasetName, List<DimensionDto> dimensions, List<MeasureDto> measures);
        void FillFactTable(string datasetName, List<DimensionDto> dimensions, List<MeasureDto> measures, DataTable values);
    }

    public class StarSchemaBuilder : StarSchemaBase, IStarSchemaBuilder
    {

        public StarSchemaBuilder(QueryBuilder queryBuilder, IDataDecorator data) 
            : base(queryBuilder, data)
        {
        }

        #region public

        public void CreateAndFillDimensionTables(string datasetName, List<DimensionDto> dimensions, DataTable values)
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
                        FillDimensionTable(datasetName, dimension, values, childDimensions);
                        existingDimensions.Add(dimension.Id);
                    }
                }
            }
        }

        public void CreateFactTable(string datasetName, List<DimensionDto> dimensions, List<MeasureDto> measures)
        {
            var rootDimensionNames = dimensions.Where(d => d.ParentDimension == null).Select(d => d.Name).ToList<string>();
            var measuresColumns = measures.Select(m => new Column {Name = m.Name, Type = ((DataType)m.Type).ToSqlType() });
            var foreignKeys = new List<ForeignKey>();
            foreignKeys.AddRange(rootDimensionNames.Select(dimensionName => new ForeignKey
            {
                KeyName = dimensionName + "Id",
                Reference = datasetName + dimensionName
            }));
            QueryBuilder.CreateTable(datasetName + "FactTable", measuresColumns, foreignKeys);
        }        

        public void FillFactTable(string datasetName, List<DimensionDto> dimensions, List<MeasureDto> measures, DataTable values)
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
                    Name = dimension.Name + "Id",
                    Value = GetDimensionId(datasetName + dimension.Name, row[(string) dimension.Name].ToString(dimension.Type)).ToString()
                }).ToList();
                
                QueryBuilder.Insert(datasetName + "FactTable", dimensionColumns.Concat(measureColumns));
            }
        }

        #endregion

        #region private

        private int GetDimensionId(string tableName, string value)
        {
            return Convert.ToInt32
                (QueryBuilder.Select(tableName, new Column { Name = "Value", Value = value }).Rows.Cast<DataRow>().First()[0]);
            
        }

        private void FillDimensionTable(string datasetName, DimensionDto dimension, DataTable values, List<DimensionDto> children)
        {
            var distinctValues = values.GetDistinctTable(dimension.Type, dimension.Name);
            var allRowsToInsert = new List<List<Column>>();
            foreach (DataRow row in distinctValues.Rows)
            {
                var dimensionColumns = new List<Column>
                {
                    new Column { Name = "Value", Value = row[(string) dimension.Name].ToString(dimension.Type) }
                };
                dimensionColumns.AddRange(children.Select(child => new Column
                {
                    Name = child.Name + "Id",
                    Value = GetDimensionId(datasetName + child.Name, row[(string) child.Name].ToString(child.Type)).ToString()
                }));
                allRowsToInsert.Add(dimensionColumns);
            }
            QueryBuilder.Insert(datasetName + dimension.Name, allRowsToInsert);
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
            QueryBuilder.CreateTable(tableName, columns, foreignKeys);
        }

        #endregion
    }
}