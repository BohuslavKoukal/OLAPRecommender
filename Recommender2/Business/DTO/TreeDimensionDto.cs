using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace Recommender2.Business.DTO
{
    public class TreeDimensionDto : DimensionDto
    {
        public int? ParentId { get; set; }
        public List<TreeDimensionDto> Children { get; set; }
        public TreeDimensionDto GetDimensionDto(int id)
        {
            return id == Id 
                ? this 
                : Children?.Select(child => child.GetDimensionDto(id)).FirstOrDefault(childInSubtree => childInSubtree != null);
        }

        public IEnumerable<int> GetSubtreeIds()
        {
            var ret = new List<int>();
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    ret.Add(child.Id);
                    ret.AddRange(child.GetSubtreeIds());
                }
            }
            return ret;
        }

        public IEnumerable<TreeDimensionDto> GetSubtreeDimensionDtos()
        {
            var ret = new List<TreeDimensionDto> { this };
            if (Children == null) return ret;
            foreach (var child in Children)
            {
                ret.AddRange(child.GetSubtreeDimensionDtos());
            }
            return ret;
        }

    }

    public class FlatDimensionDto : DimensionDto
    {
        
    }

    public abstract class DimensionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DatasetName { get; set; }
        public string TableName => DatasetName + Name;
        public string IdName => Name + "Id";
        public int Type { get; set; }
        public List<DimensionValue> DimensionValues { get; set; }

        public void Populate(IEnumerable<DimensionValue> dimensionValues)
        {
            DimensionValues = dimensionValues.ToList();
        }
    }
    
}