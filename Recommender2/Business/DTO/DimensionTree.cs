using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Business.DTO
{
    public class DimensionTree
    {
        public DimensionTree()
        {
            RootDimensions = new List<DimensionDto>();
        }

        public List<DimensionDto> RootDimensions { get; set; }

        public DimensionDto GetDimensionDto(int id)
        {
            return RootDimensions.Select(rootDimension => rootDimension.GetDimensionDto(id)).SingleOrDefault(childDimensionDto => childDimensionDto != null);
        }

        public List<int> GetDimensionIds()
        {
            var ret = new List<int>();
            foreach (var rootDimension in RootDimensions)
            {
                ret.AddRange(rootDimension.GetSubtreeIds());
            }
            return ret;
        }

        public int Count => GetDimensionIds().Count;

        public bool Contains(int id)
        {
            return GetDimensionIds().Contains(id);
        }

        public bool Contains(DimensionDto dimension)
        {
            return Contains(dimension.Id);
        }

        public void Add(DimensionDto dimension, int? parentId = null)
        {
            if (parentId == null)
            {
                RootDimensions.Add(dimension);
            }
            else
            {
                GetDimensionDto(parentId.Value).Children.Add(dimension);
            }
        }
    }

    
}