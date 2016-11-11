using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Business.DTO
{
    public class DrilldownGraphDto : GraphDto
    {
        public string DimensionName { get; set; }
    }

    public class DrilldownGraphXAxisRootDto : GraphXAxisRootDto
    {
        public override double GetValue()
        {
            return XAxisLeaves.Aggregate(0, (current, leaf) => (int) (current + ((DrilldownGraphXAxisLeafDto) leaf).Value));
        }
    }

    public class DrilldownGraphXAxisLeafDto : GraphXAxisLeafDto
    {
        public double Value { get; set; }
    }
}