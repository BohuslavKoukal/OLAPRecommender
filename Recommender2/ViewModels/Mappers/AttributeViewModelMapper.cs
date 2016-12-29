using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Business.DTO;
using Recommender.Common.Enums;

namespace Recommender2.ViewModels.Mappers
{
    public class AttributeViewModelMapper
    {
        public IEnumerable<MeasureDto> MapToMeasures(IEnumerable<AttributeViewModel> attributes)
        {
            return attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Measure).Select(attribute => new MeasureDto
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeTypeId
            }).ToList();
        }

        public IEnumerable<DimensionDto> MapToDimensions(List<AttributeViewModel> attributes)
        {
            var dimensions = attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Dimension).Select(attribute => new FlatDimensionDto
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