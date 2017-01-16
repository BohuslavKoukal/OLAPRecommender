using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business
{
    public interface IStarSchemaQuerier
    {
        List<DimensionValueDto> GetValuesOfDimension(DimensionDto dimension, Column selector = null);

        double GetFactTableSum(DimensionTree allValues, List<FlatDimensionDto> filters,
            List<FlatDimensionDto> conditions, Measure measure);

        IEnumerable<DimensionValueDto> GetCorrespondingValues (DimensionTree tree, int dimensionId, DimensionDto child);
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
                        ? ((DateTime)row["Value"]).ToString("dd.MM.yyyy") 
                        : row["Value"].ToString()
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
                newIds.AddRange(from DataRow currentId in currentIds.Rows select Convert.ToInt32(currentId["Id"]));
                childDimension = parentDimension;
            }
            var ret = new List<DimensionValueDto>();
            foreach (var newId in newIds)
            {
                ret.AddRange(GetValuesOfDimension(tree.GetDimensionDto(dimensionId), new Column { Name = "Id", Value = newId.ToString() }));
            }
            return ret;
        }

        private IEnumerable<DimensionValueIds> GroupDimensionIds(IEnumerable<DimensionValueIds> dimensionIds)
        {
            var groupedIds = new List<DimensionValueIds>();
            foreach (var dimId in dimensionIds)
            {
                var existingDim = groupedIds.SingleOrDefault(gid => gid.DimensionId == dimId.DimensionId);
                if (existingDim == null)
                {
                    groupedIds.Add(dimId);
                }
                else
                {
                    foreach (var id in dimId.ValueIds)
                    {
                        existingDim.ValueIds.Add(id);
                    }
                }
            }
            return groupedIds;
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
                newIds.AddRange(from DataRow currentId in currentIds.Rows select Convert.ToInt32(currentId["Id"]));
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