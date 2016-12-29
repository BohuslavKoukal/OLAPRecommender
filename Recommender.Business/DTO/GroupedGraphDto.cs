using System.Collections.Generic;
using System.Linq;

namespace Recommender.Business.DTO
{
    // Represents a 2-level grouped graph with root and leaf x axis values and a legend
    public class GroupedGraphDto : GraphDto
    {
        public IEnumerable<GraphLegendValueDto> GetLegendValues()
        {
            return Leaves.SelectMany(leaf => leaf.LegendValues);
        }

        private IEnumerable<GroupedGraphXAxisLeafDto> Leaves
        {
            get {
                return Roots.SelectMany(root => root.XAxisLeaves.Cast<GroupedGraphXAxisLeafDto>().ToList());
            }
        }
    }

    public class GroupedGraphXAxisRootDto : GraphXAxisRootDto
    {
        public override double GetValue()
        {
            return XAxisLeaves.Cast<GroupedGraphXAxisLeafDto>().Sum(groupedLeaf => groupedLeaf.LegendValues.Sum(v => v.Value));
        }
    }

    public class GroupedGraphXAxisLeafDto : GraphXAxisLeafDto
    {
        public List<GraphLegendValueDto> LegendValues { get; set; }
    }

    public class GraphLegendValueDto
    {
        public GraphLegendDto Legend { get; set; }
        public double Value { get; set; }
    }

    public class GraphLegendDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}