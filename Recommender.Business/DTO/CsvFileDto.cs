using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Business.DTO
{
    public class CsvFileDto
    {
        public string FilePath { get; set; }
        public virtual ICollection<AttributeDto> Attributes { get; set; }
    }
}
