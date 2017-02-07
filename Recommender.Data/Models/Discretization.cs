using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Data.Models
{
    public class Discretization
    {
        public Discretization()
        {
            Bins = new List<DiscretizeBin>();
        }

        public Measure Measure { get; set; }
        public List<DiscretizeBin> Bins { get; set; }
    }

    public class DiscretizeBin
    {
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public string Name => $"[{LeftMargin};{RightMargin}]";
    }
}
