using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender2.ViewModels.Mappers
{
    public class AttributeViewModelMapper
    {
        public IEnumerable<DimensionDto> MapToDimensionDtos(List<AttributeViewModel> attributes)
        {
            var dimensions = attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Dimension).Select(attribute => new FlatDimensionDto
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeType.ToType()
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

        public IEnumerable<Measure> MapToMeasures(IEnumerable<AttributeViewModel> attributes)
        {
            return attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Measure).Select(attribute => new Measure
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeType.ToType().ToInt()
            }).ToList();
        }

        public IEnumerable<Dimension> MapToDimensions(List<AttributeViewModel> attributes)
        {
            var dimensions = attributes.Where(a => a.SelectedAttributeRoleId == (int)AttributeRole.Dimension).Select(attribute => new Dimension
            {
                Name = attribute.Name,
                Type = attribute.SelectedAttributeType.ToType().ToInt()
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

        public IEnumerable<DimensionOrMeasureDto> MapToDimensionsAndMeasures(List<AttributeViewModel> attributes)
        {
            var dom = new List<DimensionOrMeasureDto>();
            foreach (var attribute in attributes)
            {
                dom.Add(new DimensionOrMeasureDto
                {
                    Name = attribute.Name,
                    Type = attribute.SelectedAttributeType.ToType()
                });
            }
            return dom;
        }
    }
}