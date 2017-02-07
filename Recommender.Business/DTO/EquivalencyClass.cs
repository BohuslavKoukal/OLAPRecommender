using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Business.DTO
{
    public class EquivalencyClass
    {
        public string Name { get; set; }
        public List<DimensionDto> Dimensions { get; set; }
    }
}
