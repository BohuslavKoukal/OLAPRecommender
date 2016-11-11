using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business.DTO;
using Recommender2.DataAccess;
using Recommender2.Models;

namespace Recommender2.Business.Service
{
    public class GraphService
    {
        private readonly StarSchemaQuerier _querier;

        public GraphService(StarSchemaQuerier querier)
        {
            _querier = querier;
        }

        public GroupedGraphDto GetGroupedGraph(Dimension xDimension, Dimension legendDimension, Measure measure)
        {
            var graph = new GroupedGraphDto
            {
                Name = $"Grouped graph of {measure.Name} by {xDimension.Name} and {legendDimension.Name}",
                Roots = new List<GraphXAxisRootDto>()
            };
            
            // if x-dimension is root dimension, its values will be leaves
            if (xDimension.ParentDimension == null)
            {
                graph.Roots.Add(GetRoot(xDimension, measure, null, legendDimension));
            }
            // otherwise its values will be root and its parents values will be leaves
            else
            {
                var xDimValues = _querier.GetValuesOfDimension(xDimension);
                foreach (var xValue in xDimValues)
                {
                    graph.Roots.Add(GetParentRoot(xDimension, measure, xValue, legendDimension));
                }
            }
            return graph;
        }

        public DrilldownGraphDto GetDrilldownGraph(Dimension xDimension, Measure measure)
        {
            var graph = new DrilldownGraphDto
            {
                DimensionName = xDimension.Name,
                Name = $"Drilldown graph of {measure.Name} by {xDimension.Name}",
                Roots = new List<GraphXAxisRootDto>()
            };
            // if x-dimension is root dimension, there is no point for showing drilldown graph
            if (xDimension.ParentDimension == null)
            {
                return null;
            }
            // otherwise values of x dimension will be root and its parents values will be leaves
            else
            {
                var xDimValues = _querier.GetValuesOfDimension(xDimension);
                foreach (var xValue in xDimValues)
                {
                    graph.Roots.Add(GetParentRoot(xDimension, measure, xValue));
                }
            }
            return graph;
        }

        private GroupedGraphXAxisRootDto GetRoot(Dimension xDimension, Measure measure, DimensionValue xValue = null, Dimension legendDimension = null)
        {
            var xAxisRoot = new GroupedGraphXAxisRootDto
            {
                Id = xValue?.Id ?? 0,
                Name = xValue?.Value ?? string.Empty,
                XAxisLeaves = new List<GraphXAxisLeafDto>()
            };
            var xDimValues = _querier.GetValuesOfDimension(xDimension);
            foreach (var dimValue in xDimValues)
            {
                var leaf = GetLeaf(xDimension, dimValue, measure, legendDimension);
                xAxisRoot.XAxisLeaves.Add(leaf);
            }
            return xAxisRoot;
        }

        private GroupedGraphXAxisRootDto GetParentRoot(Dimension childDimension, Measure measure, DimensionValue xValue, Dimension legendDimension)
        {
            var xAxisRoot = new GroupedGraphXAxisRootDto
            {
                Id = xValue.Id,
                Name = xValue.Value,
                XAxisLeaves = new List<GraphXAxisLeafDto>()
            };
            var xDimValues = _querier.GetValuesOfDimension(childDimension.ParentDimension,
                new Column { Name = childDimension.IdName, Value = xValue.Id.ToString()});
            foreach (var dimValue in xDimValues)
            {
                var leaf = GetLeaf(childDimension.ParentDimension, dimValue, measure, legendDimension);
                xAxisRoot.XAxisLeaves.Add(leaf);
            }
            return xAxisRoot;
        }

        private DrilldownGraphXAxisRootDto GetParentRoot(Dimension childDimension, Measure measure, DimensionValue xValue)
        {
            var xAxisRoot = new DrilldownGraphXAxisRootDto
            {
                Id = xValue.Id,
                Name = xValue.Value,
                XAxisLeaves = new List<GraphXAxisLeafDto>()
            };
            var xDimValues = _querier.GetValuesOfDimension(childDimension.ParentDimension,
                new Column { Name = childDimension.IdName, Value = xValue.Id.ToString() });
            foreach (var dimValue in xDimValues)
            {
                var leaf = GetLeaf(childDimension.ParentDimension, dimValue, measure);
                xAxisRoot.XAxisLeaves.Add(leaf);
            }
            return xAxisRoot;
        }

        private GraphXAxisLeafDto GetLeaf(Dimension xDimension, DimensionValue xValue, Measure measure, Dimension legendDimension = null)
        {
            if (legendDimension == null)
            {
                return new DrilldownGraphXAxisLeafDto
                {
                    Id = xValue.Id,
                    Name = xValue.Value,
                    Value = _querier.GetFactTableSum(new DimensionValue { Dimension = xDimension, Id = xValue.Id }, measure)
                };
            }
            var legendValues = _querier.GetValuesOfDimension(legendDimension);
            var leaf = new GroupedGraphXAxisLeafDto
            {
                Id = xValue.Id,
                Name = xValue.Value,
                LegendValues = new List<GraphLegendValueDto>()
            };
            foreach (var legendValue in legendValues)
            {
                var dimensionValues = new List<DimensionValue>
                {
                    new DimensionValue {Dimension = xDimension, Id = xValue.Id},
                    new DimensionValue {Dimension = legendDimension, Id = legendValue.Id}
                };
                leaf.LegendValues.Add(new GraphLegendValueDto
                {
                    Legend = new GraphLegendDto
                    {
                        Id = legendValue.Id,
                        Name = legendValue.Value
                    },
                    Value = _querier.GetFactTableSum(dimensionValues, measure)
                });
            }
            return leaf;
        }





    }
}