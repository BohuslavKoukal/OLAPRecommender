using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Business.DTO
{
    public abstract class DimensionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DatasetName { get; set; }
        public string TableName => DatasetName + Name;
        public string IdName => Name + "Id";
        public int Type { get; set; }
        public List<DimensionValueDto> DimensionValues { get; set; }
        public DatasetDto DataSet { get; set; }
        public DimensionDto ParentDimension { get; set; }

        public void Populate(IEnumerable<DimensionValueDto> dimensionValues)
        {
            DimensionValues = dimensionValues.ToList();
        }
    }
}
