using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Recommender.Business.DTO;
using Recommender.Common.Constants;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;

namespace Recommender.Business.StarSchema
{
    public interface IStarSchemaQuerier
    {
        List<DimensionValueDto> GetValuesOfDimension(DimensionDto dimension, Column selector = null);

        double GetFactTableSum(DimensionTree allValues, List<FlatDimensionDto> filters,
            List<FlatDimensionDto> conditions, Measure measure);

        IEnumerable<DimensionValueDto> GetCorrespondingValues (DimensionTree tree, int dimensionId, DimensionDto child);

        int GetFactTableRowCount(string factTableName);

        List<double> GetOrderedMeasureValues(Measure measure);
    }

    public class StarSchemaQuerier : StarSchemaBase, IStarSchemaQuerier
    {
        public StarSchemaQuerier(IQueryBuilder queryBuilder, IDataAccessLayer data) 
            : base(queryBuilder, data)
        {
        }

        public List<DimensionValueDto> GetValuesOfDimension(DimensionDto dimension, Column selector = null)
        {
            DataTable table = selector == null 
                    ? QueryBuilder.Select(dimension.TableName, new List<Column>()) 
                    : QueryBuilder.Select(dimension.TableName, selector);
            var valuesToReturn = (from DataRow row
                    in table.Rows
                    select new DimensionValueDto
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Value = dimension.Type == typeof(DateTime)
                        ? ((DateTime)row[Constants.String.Value]).ToString("dd.MM.yyyy") 
                        : row[Constants.String.Value].ToString()
                    })
                    .ToList();
            return valuesToReturn;
        }

        public double GetFactTableSum(DimensionTree allValues, List<FlatDimensionDto> filters,
            List<FlatDimensionDto> conditions, Measure measure)
        {
            var factTableName = allValues.FactTableName;
            var filterRootValueIds = new List<List<DimensionValueIds>>();
            var conditionRootValueIds = new List<List<DimensionValueIds>>();
            foreach (var filter in filters)
            {
                filterRootValueIds.Add(filter.DimensionValues.Select(value => GetRootDimensionIds(allValues, filter.Id, value.Id)).ToList());
            }
            foreach (var condition in conditions)
            {
                conditionRootValueIds.Add(condition.DimensionValues.Select(value => GetRootDimensionIds(allValues, condition.Id, value.Id)).ToList());
            }
            
            var allFilteringIds = filterRootValueIds.Concat(conditionRootValueIds);
            var selectors = ConvertToSelectors(allValues, allFilteringIds);
            var queryResult = QueryBuilder.Select(factTableName, selectors);
            return AggregateData(queryResult, measure.Name);
        }

        public int GetFactTableRowCount(string factTableName)
        {
            return QueryBuilder.Count(factTableName);
        }

        public List<double> GetOrderedMeasureValues(Measure measure)
        {
            var dt = QueryBuilder.Select(measure.DataSet.GetViewName(), new List<Column>());
            var ret = (from DataRow row
                    in dt.Rows select Convert.ToDouble(row[measure.Name]))
                    .ToList();
            return ret.OrderBy(d => d).ToList();
        }

        private List<List<Column>> ConvertToSelectors(DimensionTree allValues, IEnumerable<List<DimensionValueIds>> filters)
        {
            var selectors = new List<List<Column>>();
            foreach (var filter in filters)
            {
                var listToAdd = new List<Column>();
                foreach (var valueIds in filter)
                {
                    listToAdd.AddRange(valueIds.ValueIds.Select(dId => new Column { Name = allValues.GetDimensionDto(valueIds.DimensionId).IdName, Value = dId.ToString() }));
                }
                selectors.Add(listToAdd);
            }
            return selectors;
        }

        public IEnumerable<DimensionValueDto> GetCorrespondingValues(DimensionTree tree, int dimensionId, DimensionDto child)
        {
            var childDimension = tree.GetDimensionDto(child.Id);
            var oldIds = new List<int>();
            oldIds.AddRange(child.DimensionValues.Select(dv => dv.Id));
            var newIds = new List<int>();
            while (childDimension.Id != dimensionId)
            {
                var parentDimension = tree.GetDimensionDto((int)childDimension.ParentId);
                var selectors = new List<List<Column>>
                    {
                        oldIds.Select(childId => new Column {Name = childDimension.IdName, Value = childId.ToString()})
                            .ToList()
                    };
                oldIds = newIds;
                var currentIds = QueryBuilder.Select(parentDimension.TableName, selectors);
                newIds.Clear();
                newIds.AddRange(from DataRow currentId in currentIds.Rows select Convert.ToInt32(currentId[Constants.String.Id]));
                childDimension = parentDimension;
            }
            var ret = new List<DimensionValueDto>();
            foreach (var newId in newIds)
            {
                ret.AddRange(GetValuesOfDimension(tree.GetDimensionDto(dimensionId), new Column { Name = Constants.String.Id, Value = newId.ToString() }));
            }
            return ret;
        }

        private double AggregateData(DataTable table, string columnName)
        {
            return Math.Round(table.Rows.Cast<DataRow>().Sum(row => Convert.ToDouble(row[columnName])), 4);
        }

        // Returns root dimension and list of its ids for concrete non-root dimension
        private DimensionValueIds GetRootDimensionIds(DimensionTree tree, int dimensionId, int valueId)
        {
            var isRoot = tree.IsRoot(dimensionId);
            var childDimension = tree.GetDimensionDto(dimensionId);
            var oldIds = new List<int> { valueId };
            var newIds = new List<int> { valueId };
            while (!isRoot)
            {
                var parentDimension = tree.GetDimensionDto((int) childDimension.ParentId);
                var selectors = new List<List<Column>>
                    {
                        oldIds.Select(childId => new Column {Name = childDimension.IdName, Value = childId.ToString()})
                            .ToList()
                    };
                oldIds = newIds;

                var currentIds = QueryBuilder.Select(parentDimension.TableName, selectors);
                newIds.Clear();
                newIds.AddRange(from DataRow currentId in currentIds.Rows select Convert.ToInt32(currentId[Constants.String.Id]));
                childDimension = parentDimension;
                if (childDimension.ParentId == null)
                    isRoot = true;
            }
            return new DimensionValueIds
            {
                DimensionId = childDimension.Id,
                ValueIds = new HashSet<int>(newIds)
            };
        }
    }
}