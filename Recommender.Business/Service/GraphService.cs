using System.Collections.Generic;
using System.Linq;
using Recommender.Business.DTO;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.Service
{
    public interface IGraphService
    {
        GroupedGraphDto GetGroupedGraph(DimensionTree allDimensionsTree, TreeDimensionDto xDimension,
            DimensionDto legendDimension, MeasureDto measure, List<FlatDimensionDto> filterDimensions);

        DrilldownGraphDto GetDrilldownGraph(DimensionTree allDimensionsTree, TreeDimensionDto xDimension, MeasureDto measure, List<FlatDimensionDto> filters);
    }

    public class GraphService : IGraphService
    {
        private readonly IStarSchemaQuerier _querier;

        public GraphService(IStarSchemaQuerier querier)
        {
            _querier = querier;
        }

        public GroupedGraphDto GetGroupedGraph(DimensionTree allDimensionsTree, TreeDimensionDto xDimension,
            DimensionDto legendDimension, MeasureDto measure, List<FlatDimensionDto> filterDimensions)
        {
            var graph = new GroupedGraphDto
            {
                Name = $"Grouped graph of {measure.Name} by {xDimension.Name} and {legendDimension.Name}",
                Roots = new List<GraphXAxisRootDto>()
            };
            var xDimIsRoot = allDimensionsTree.IsRoot(xDimension.Id);
            var filteredXValues = GetFilteredValues(allDimensionsTree, xDimension, filterDimensions);
            // if x-dimension is root dimension, its values will be leaves
            if (xDimIsRoot)
            {
                graph.Roots.Add(GetRoot(allDimensionsTree, xDimension, measure, filteredXValues, filterDimensions, null, legendDimension));
            }
            // otherwise its values will be root and its parents values will be leaves
            else
            {
                foreach (var xValue in filteredXValues)
                {
                    graph.Roots.Add(GetParentRoot(allDimensionsTree, xDimension, measure, xValue, filterDimensions, legendDimension));
                }
            }
            return graph;
        }

        public DrilldownGraphDto GetDrilldownGraph(DimensionTree allDimensionsTree, TreeDimensionDto xDimension, MeasureDto measure, List<FlatDimensionDto> filters)
        {
            var graph = new DrilldownGraphDto
            {
                DimensionName = xDimension.Name,
                Name = $"Drilldown graph of {measure.Name} by {xDimension.Name}",
                Roots = new List<GraphXAxisRootDto>()
            };
            var xDimIsRoot = allDimensionsTree.IsRoot(xDimension.Id);
            // if x-dimension is root dimension, there is no point for showing drilldown graph
            if (xDimIsRoot)
            {
                return null;
            }
            // otherwise values of x dimension will be root and its parents values will be leaves
            else
            {
                var filteredXValues = GetFilteredValues(allDimensionsTree, xDimension, filters);
                foreach (var xValue in filteredXValues)
                {
                    graph.Roots.Add(GetParentRoot(allDimensionsTree, xDimension, measure, xValue, filters));
                }
            }
            return graph;
        }

        private GroupedGraphXAxisRootDto GetRoot(DimensionTree allDimensionsTree, DimensionDto xDimension, MeasureDto measure,
            IEnumerable<DimensionValueDto> filteredXValues, List<FlatDimensionDto> filters,
            DimensionValueDto xValue = null, DimensionDto legendDimension = null)
        {
            var xAxisRoot = new GroupedGraphXAxisRootDto
            {
                Id = xValue?.Id ?? 0,
                Name = xValue?.Value ?? string.Empty,
                XAxisLeaves = new List<GraphXAxisLeafDto>()
            };
            foreach (var dimValue in filteredXValues)
            {
                var leaf = GetLeaf(allDimensionsTree, xDimension, dimValue, measure, filters, legendDimension);
                xAxisRoot.XAxisLeaves.Add(leaf);
            }
            return xAxisRoot;
        }

        private GraphXAxisRootDto GetParentRoot(DimensionTree allDimensionsTree, TreeDimensionDto childDimension,
            MeasureDto measure, DimensionValueDto xValue, List<FlatDimensionDto> filters, DimensionDto legendDimension = null)
        {
            GraphXAxisRootDto xAxisRoot;
            if (legendDimension == null)
            {
                xAxisRoot = new DrilldownGraphXAxisRootDto
                {
                    Id = xValue.Id,
                    Name = xValue.Value,
                    XAxisLeaves = new List<GraphXAxisLeafDto>()
                };
            }
            else
            {
                xAxisRoot = new GroupedGraphXAxisRootDto
                {
                    Id = xValue.Id,
                    Name = xValue.Value,
                    XAxisLeaves = new List<GraphXAxisLeafDto>()
                };
            }
            var parentDimension = allDimensionsTree.GetDimensionDto((int) childDimension.ParentId);
            var xDimValues = _querier.GetValuesOfDimension(
                parentDimension, new Column { Name = childDimension.IdName, Value = xValue.Id.ToString()});
            var filteredValues = GetFilteredValues(allDimensionsTree, parentDimension, filters, xDimValues);
            foreach (var dimValue in filteredValues)
            {
                var leaf = GetLeaf(allDimensionsTree, allDimensionsTree.GetDimensionDto((int)childDimension.ParentId), dimValue, measure, filters, legendDimension);
                xAxisRoot.XAxisLeaves.Add(leaf);
            }
            return xAxisRoot;
        }

        private GraphXAxisLeafDto GetLeaf(DimensionTree allDimensionsTree, DimensionDto xDimension, DimensionValueDto xValue, MeasureDto measure, 
            List<FlatDimensionDto> filters, DimensionDto legendDimension = null)
        {
            if (legendDimension == null)
            {
                var conditionDto = new FlatDimensionDto
                {
                    Id = xDimension.Id,
                    DatasetName = xDimension.DatasetName,
                    Name = xDimension.Name,
                    DimensionValues = new List<DimensionValueDto> { new DimensionValueDto { Id = xValue.Id } }
                };
                return new DrilldownGraphXAxisLeafDto
                {
                    Id = xValue.Id,
                    Name = xValue.Value,
                    Value = _querier.GetFactTableSum(allDimensionsTree,
                        filters, new[] {conditionDto}.ToList(), measure)
                };
            }
            //var legendValues = _querier.GetValuesOfDimension(legendDimension);
            var leaf = new GroupedGraphXAxisLeafDto
            {
                Id = xValue.Id,
                Name = xValue.Value,
                LegendValues = new List<GraphLegendValueDto>()
            };
            var legendFilteredValues = GetFilteredValues(allDimensionsTree, legendDimension, filters, legendDimension.DimensionValues);
            foreach (var legendValue in legendFilteredValues)
            {
                var xDimensionDto = new FlatDimensionDto
                {
                    Id = xDimension.Id,
                    DatasetName = xDimension.DatasetName,
                    Name = xDimension.Name,
                    DimensionValues = new[] { new DimensionValueDto { Id = xValue.Id } }.ToList()
                };
                var legendDimensionDto = new FlatDimensionDto
                {
                    Id = legendDimension.Id,
                    DatasetName = legendDimension.DatasetName,
                    Name = legendDimension.Name,
                    DimensionValues = new[] { new DimensionValueDto {Id = legendValue.Id}}.ToList()
                };
                leaf.LegendValues.Add(new GraphLegendValueDto
                {
                    Legend = new GraphLegendDto
                    {
                        Id = legendValue.Id,
                        Name = legendValue.Value
                    },
                    Value = _querier.GetFactTableSum(allDimensionsTree, filters, new[]{ xDimensionDto, legendDimensionDto }.ToList(), measure)
                });
            }
            return leaf;
        }

        private IEnumerable<DimensionValueDto> GetFilteredValues(DimensionTree allDimensionsTree, DimensionDto dimension,
            List<FlatDimensionDto> filterDimensions, List<DimensionValueDto> dimensionValues = null)
        {
            var dimensionFilter = filterDimensions.SingleOrDefault(fd => fd.Id == dimension.Id);
            if(dimensionValues == null)
                dimensionValues = _querier.GetValuesOfDimension(dimension);
            var childrenInFilters = filterDimensions.Where(fd => allDimensionsTree.GetDimensionDto(dimension.Id).GetSubtreeIds().Contains(fd.Id)).ToList();
            var valuesFromFilter = new List<DimensionValueDto>();
            var valuesFromAncestors = new List<DimensionValueDto>();
            if (dimensionFilter != null)
            {
                valuesFromFilter =
                    dimensionFilter.DimensionValues
                        .Select(filterValue => dimensionValues.SingleOrDefault(v => v.Id == filterValue.Id))
                        .Where(v => v != null).ToList();
            }
            if (childrenInFilters.Any())
            {
                foreach (var child in childrenInFilters)
                {
                    var dimensionCorrespondingValues = _querier.GetCorrespondingValues(allDimensionsTree, dimension.Id, child);
                    foreach (var correspondingValue in dimensionCorrespondingValues)
                    {
                        if(dimensionValues.Select(dv => dv.Id).Contains(correspondingValue.Id))
                            valuesFromAncestors.Add(correspondingValue);
                    }
                }
            }
            if (valuesFromFilter.Any() && valuesFromAncestors.Any())
            {
                var filteredValues = valuesFromFilter.Select(v1 => v1.Id)
                    .Where(v => valuesFromAncestors.Select(v2 => v2.Id)
                    .Contains(v)).ToList();
                return filteredValues.Any() ? dimensionValues.Where(xdv => filteredValues.Contains(xdv.Id)) : dimensionValues;
            }
            else if (valuesFromFilter.Any())
            {
                return valuesFromFilter;
            }
            else if (valuesFromAncestors.Any())
            {
                return valuesFromAncestors;
            }
            return dimensionValues;
        }

    }
}