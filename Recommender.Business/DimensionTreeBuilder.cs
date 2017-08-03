using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Recommender.Business.DTO;
using Recommender.Business.StarSchema;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;

namespace Recommender.Business
{
    public interface IDimensionTreeBuilder
    {
        DimensionTree ConvertToTree(int datasetId, bool populate = false);
        DimensionTree ConvertToTree(IEnumerable<Dimension> dimensions, string prefix, bool populate = false);
        List<SelectListItem> ConvertTreeToSelectList(DimensionTree tree);
    }

    public class DimensionTreeBuilder : IDimensionTreeBuilder
    {
        public List<SelectListItem> Items;
        private readonly IDataAccessLayer _data;
        private readonly IStarSchemaQuerier _starSchemaQuerier;

        public DimensionTreeBuilder(IDataAccessLayer data, IStarSchemaQuerier querier)
        {
            Items = new List<SelectListItem>();
            _data = data;
            _starSchemaQuerier = querier;
        }

        public DimensionTree ConvertToTree(int datasetId, bool populate = false)
        {
            var dataset = _data.GetDataset(datasetId);
            return ConvertToTree(dataset.Dimensions.ToList(), dataset.GetPrefix(), populate);
        }

        public DimensionTree ConvertToTree(IEnumerable<Dimension> dimensions, string prefix, bool populate = false)
        {
            var dimensionList = dimensions.ToList();
            var dimensionTree = new DimensionTree(dimensionList.First().DataSet.Name, prefix);
            while (dimensionTree.Count != dimensionList.Count)
            {
                foreach (var dimension in dimensionList)
                {
                    var dimToAdd = new TreeDimensionDto
                    {
                        Children = new List<TreeDimensionDto>(),
                        Id = dimension.Id,
                        Name = dimension.Name,
                        ParentId = dimension.ParentDimension?.Id,
                        DatasetName = dimension.DataSet.Name,
                        Type = dimension.Type.ToType(),
                        Prefix = prefix
                    };
                    if (!dimensionTree.Contains(dimToAdd))
                    {
                        if (dimension.ParentDimension == null)
                        {
                            dimensionTree.Add(dimToAdd);
                        }
                        else
                        {
                            if (dimensionTree.Contains((int) dimension.ParentDimension.Id))
                            {
                                dimensionTree.Add(dimToAdd);
                            }
                        }
                    }
                }
            }
            if (populate)
                Populate(dimensionTree);
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

        private void Populate(DimensionTree tree)
        {
            foreach (var dimensionDto in tree.GetDimensionDtos())
            {
                var dimensionValues = _starSchemaQuerier.GetValuesOfDimension(dimensionDto);
                dimensionDto.Populate(dimensionValues);
            }
        }

        private void AddSelectListItem(TreeDimensionDto treeDimension, int level)
        {
            var prefix = string.Empty;
            for (var i = 0; i < level; i++)
            {
                prefix += "- ";
            }
            Items.Add(new SelectListItem
            {
                Value = treeDimension.Id.ToString(),
                Text = prefix + treeDimension.Name
            }); 

            foreach (var child in treeDimension.Children)
            {
                AddSelectListItem(child, level + 1); //<-- recursive
            }
        }


    }
}