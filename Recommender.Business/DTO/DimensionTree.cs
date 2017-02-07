using System.Collections.Generic;
using System.Linq;
using Recommender.Common.Constants;

namespace Recommender.Business.DTO
{
    public class DimensionTree
    {
        public DimensionTree(string datasetName)
        {
            RootDimensions = new List<TreeDimensionDto>();
            DatasetName = datasetName;
        }

        public List<TreeDimensionDto> RootDimensions { get; set; }
        public string DatasetName { get; set; }
        public int Count => GetDimensionIds().Count();
        public string FactTableName => DatasetName + Constants.String.FactTable;

        public TreeDimensionDto GetDimensionDto(int id)
        {
            return RootDimensions
                .Select(rootDimension => rootDimension.GetDimensionDto(id))
                .SingleOrDefault(childDimensionDto => childDimensionDto != null);
        }

        public bool IsRoot(int id)
        {
            return RootDimensions.Select(d => d.Id).Contains(id);
        }

        public IEnumerable<int> GetDimensionIds()
        {
            var ret = new List<int>();
            foreach (var rootDimension in RootDimensions)
            {
                ret.Add(rootDimension.Id);
                ret.AddRange(rootDimension.GetSubtreeIds());
            }
            return ret;
        }

        public IEnumerable<TreeDimensionDto> GetDimensionDtos()
        {
            var ret = new List<TreeDimensionDto>();
            foreach (var rootDimension in RootDimensions)
            {
                ret.AddRange(rootDimension.GetSubtreeDimensionDtos());
            }
            return ret;
        }

        public IEnumerable<int> GetAncestorsIds(int id)
        {
            return GetDimensionDtos()
                .Where(d => d.GetSubtreeIds().Contains(id))
                .Select(d => d.Id);
        }

        public bool Contains(int id)
        {
            return GetDimensionIds().Contains(id);
        }

        public bool Contains(TreeDimensionDto treeDimension)
        {
            return Contains(treeDimension.Id);
        }

        public void Add(TreeDimensionDto treeDimension)
        {
            if (treeDimension.ParentId == null)
            {
                RootDimensions.Add(treeDimension);
            }
            else
            {
                var parentDimension = GetDimensionDto(treeDimension.ParentId.Value);
                if(parentDimension.Children == null)
                    parentDimension.Children = new List<TreeDimensionDto>();
                parentDimension.Children.Add(treeDimension);
            }
        }
    }

    
}