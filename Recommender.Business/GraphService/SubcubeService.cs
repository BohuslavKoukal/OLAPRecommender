using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Data.DataAccess;

namespace Recommender.Business.GraphService
{
    public class SubcubeService
    {
        private readonly IDataAccessLayer _data;
        public SubcubeService(IDataAccessLayer data)
        {
            _data = data;
        }

        public List<FlatDimensionDto> GetFilters(Dictionary<int, Dictionary<int, bool>> filterDimensions)
        {
            var ret = new List<FlatDimensionDto>();
            // do not filter dimensions where all values are true
            foreach (var fd in filterDimensions)
            {
                var dimension = _data.GetDimension(fd.Key);
                var dimensionDto = new FlatDimensionDto
                {
                    Id = dimension.Id,
                    Name = dimension.Name,
                    DimensionValues = new List<DimensionValueDto>()
                };
                // do not include dimensions with all values checked
                if (!fd.Value.Select(v => v.Value).Contains(false)) continue;
                foreach (var value in fd.Value)
                {
                    if (value.Value)
                        dimensionDto.DimensionValues.Add(new DimensionValueDto { Id = value.Key });
                }
                ret.Add(dimensionDto);
            }
            return ret;
        }
    }
}
