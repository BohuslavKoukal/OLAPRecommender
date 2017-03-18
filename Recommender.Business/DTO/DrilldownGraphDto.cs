using System.Linq;

namespace Recommender.Business.DTO
{
    public class DrilldownGraphDto : GraphDto
    {
        public string DimensionName { get; set; }
    }

    public class DrilldownGraphXAxisRootDto : GraphXAxisRootDto
    {
        public override double GetValue()
        {
            return XAxisLeaves.Aggregate(0L, (current, leaf) => (long) (current + ((DrilldownGraphXAxisLeafDto) leaf).Value));
        }
    }

    public class DrilldownGraphXAxisLeafDto : GraphXAxisLeafDto
    {
        public double Value { get; set; }
    }
}