using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Constants;

namespace Recommender.Business.DTO
{
    public abstract class DimensionDto : DimensionOrMeasureDto
    {
        public string DatasetName { get; set; }
        public string Prefix { get; set; }
        public string TableName => Prefix + DatasetName + Name;
        public string IdName => Name + Constants.String.Id;
        public List<DimensionValueDto> DimensionValues { get; set; }
        public DimensionDto ParentDimension { get; set; }
        public FilledFrom FilledFrom { get; set; }
        

        public void Populate(IEnumerable<DimensionValueDto> dimensionValues)
        {
            DimensionValues = dimensionValues.ToList();
        }
    }

    public enum FilledFrom
    {
        Dsd = 1,
        Data = 2
    }
}
