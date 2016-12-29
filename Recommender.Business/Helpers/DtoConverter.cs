using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Data.Models;
using Attribute = Recommender.Data.Models.Attribute;

namespace Recommender.Business.Helpers
{
    public static class DtoConverter
    {
        public static DimensionDto ToDto(this Dimension dimension)
        {
            return new FlatDimensionDto
            {
                Name = dimension.Name,
                DataSet = dimension.DataSet.ToDto(),
                DatasetName = dimension.DataSet.Name,
                Id = dimension.Id,
                ParentDimension = dimension.ParentDimension?.ToDto(),
                Type = dimension.Type,
            };
        }

        public static DatasetDto ToDto(this Dataset dataset)
        {
            return new DatasetDto
            {
                Name = dataset.Name,
                Attributes = dataset.Attributes.Select(a => a.ToDto()),
                CsvFilePath = dataset.CsvFilePath,
                Dimensions = dataset.Dimensions.Select(d => d.ToDto()),
                Id = dataset.Id,
                Measures = dataset.Measures.Select(m => m.ToDto()),
                State = dataset.State
            };
        }

        public static MeasureDto ToDto(this Measure measure)
        {
            return new MeasureDto
            {
                Id = measure.Id,
                Name = measure.Name,
                Type = measure.Type,
                DataSet = measure.DataSet.ToDto()
            };
        }

        public static AttributeDto ToDto(this Attribute attribute)
        {
            return new AttributeDto
            {
                Name = attribute.Name,
                Id = attribute.Id,
                DatasetDto = attribute.DataSet.ToDto()
            };
        }

        public static Dimension FromDto(this DimensionDto dimension)
        {
            return new Dimension
            {
                DataSet = dimension.DataSet.FromDto(),
                Name = dimension.Name,
                Id = dimension.Id,
                ParentDimension = dimension.ParentDimension?.FromDto(),
                Type = dimension.Type
            };
        }

        public static Dataset FromDto(this DatasetDto dataset)
        {
            return new Dataset
            {
                Name = dataset.Name,
                Id = dataset.Id,
                Attributes = dataset.Attributes.Select(a => a.FromDto()).ToList(),
                Dimensions = dataset.Dimensions.Select(d => d.FromDto()).ToList(),
                CsvFilePath = dataset.CsvFilePath,
                State = dataset.State,
                Measures = dataset.Measures.Select(m => m.FromDto()).ToList()
            };
        }

        public static Measure FromDto(this MeasureDto measure)
        {
            return new Measure
            {
                Name = measure.Name,
                Id = measure.Id,
                Type = measure.Type,
                DataSet = measure.DataSet.FromDto()
            };
        }

        public static Attribute FromDto(this AttributeDto attribute)
        {
            return new Attribute
            {
                Id = attribute.Id,
                Name = attribute.Name,
                DataSet = attribute.DatasetDto.FromDto()
            };
        }
    }
}
