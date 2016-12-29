using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Enums;

namespace Recommender.Business.DTO
{
    public class DatasetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CsvFilePath { get; set; }
        public State State { get; set; }

        public IEnumerable<AttributeDto> Attributes { get; set; }
        public IEnumerable<DimensionDto> Dimensions { get; set; }
        public IEnumerable<MeasureDto> Measures { get; set; }
    }
}
