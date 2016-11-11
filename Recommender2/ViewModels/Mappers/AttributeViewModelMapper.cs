using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business.Enums;
using Recommender2.Models;

namespace Recommender2.ViewModels.Mappers
{
    public class AttributeViewModelMapper
    {
        public IEnumerable<Measure> MapToMeasures(IEnumerable<AttributeViewModel> attributes)
        {
            return attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Measure).Select(attribute => new Measure
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeTypeId
            }).ToList();
        }

        public IEnumerable<Dimension> MapToDimensions(List<AttributeViewModel> attributes)
        {
            var dimensions = attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Dimension).Select(attribute => new Dimension
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeTypeId
            }).ToList();
            foreach (var dimension in dimensions)
            {
                var parentName = attributes.Single(a => a.Name == dimension.Name).SelectedAttributeParentName;
                if (!string.IsNullOrEmpty(parentName))
                {
                    dimension.ParentDimension = dimensions.Single(d => d.Name == parentName);
                }
            }
            return dimensions;
        }
    }
}