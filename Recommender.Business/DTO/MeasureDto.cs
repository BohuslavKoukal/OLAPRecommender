using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Business.DTO
{
    public class MeasureDto
    {
        public DatasetDto DataSet { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
