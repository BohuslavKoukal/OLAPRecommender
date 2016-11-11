using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Business.DTO
{

    public class DimensionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DimensionDto> Children { get; set; }
        public DimensionDto GetDimensionDto(int id)
        {
            return Id == id
                ? this
                : Children.Select(dimensionDto => dimensionDto.GetDimensionDto(id)).SingleOrDefault(subTreeResult => subTreeResult != null);
        }

        public IEnumerable<int> GetSubtreeIds()
        {
            var ret = new List<int> { Id };
            foreach (var child in Children)
            {
                ret.AddRange(child.GetSubtreeIds());
            }
            return ret;
        }
    }
    
}