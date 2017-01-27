using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Business.DTO.Mappers
{
    public static class DimensionValueMapper
    {
        public static IEnumerable<DimensionValue> ConvertToDimensionValues(List<DimensionDto> dimensionDtos)
        {
            var dimensions = DimensionMapper.ConvertToDimensions(dimensionDtos);
            foreach (var dimDto in dimensionDtos)
            {
                foreach (var value in dimDto.DimensionValues)
                {
                    yield return new DimensionValue
                    {
                        Id = value.Id,
                        Dimension = dimensions.Single(d => d.Id == dimDto.Id),
                        Value = value.Value
                    };
                }
            }
        }
    }
}
