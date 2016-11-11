using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Recommender2.Business.DTO;
using Recommender2.Models;

namespace Recommender2.Business.Helpers
{
    public class DimensionTreeBuilder
    {
        public List<SelectListItem> Items;

        public DimensionTreeBuilder()
        {
            Items = new List<SelectListItem>();
        }

        public DimensionTree ConvertToTree(List<Dimension> dimensions)
        {
            var dimensionTree = new DimensionTree();
            while (dimensionTree.Count != dimensions.Count)
            {
                foreach (var dimension in dimensions)
                {
                    var dimToAdd = new DimensionDto
                    {
                        Children = new List<DimensionDto>(),
                        Id = dimension.Id,
                        Name = dimension.Name
                    };
                    if (!dimensionTree.Contains(dimToAdd))
                    {
                        if (dimension.ParentDimension == null)
                        {
                            dimensionTree.Add(dimToAdd);
                        }
                        else
                        {
                            if (dimensionTree.Contains(dimension.ParentDimension.Id))
                            {
                                dimensionTree.Add(dimToAdd, dimension.ParentDimension.Id);
                            }
                        }
                    }
                }
            }
            return dimensionTree;
        }

        public List<SelectListItem> ConvertTreeToSelectList(DimensionTree tree)
        {
            Items.Clear();
            foreach (var rootDimension in tree.RootDimensions)
            {
                AddSelectListItem(rootDimension, 0);
            }
            return Items;
        }

        private void AddSelectListItem(DimensionDto dimension, int level)
        {
            var prefix = string.Empty;
            for (var i = 0; i < level; i++)
            {
                prefix += "- ";
            }
            Items.Add(new SelectListItem
            {
                Value = dimension.Id.ToString(),
                Text = prefix + dimension.Name
            }); 

            foreach (var child in dimension.Children)
            {
                AddSelectListItem(child, level + 1); //<-- recursive
            }
        }


    }
}