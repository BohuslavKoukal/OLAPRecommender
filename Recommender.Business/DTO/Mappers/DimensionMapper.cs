using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Business.DTO.Mappers
{
    public static class DimensionMapper
    {
        public static List<Dimension> ConvertToDimensions(List<DimensionDto> dimensionDtos)
        {
            var ret = dimensionDtos.Select(dimDto => new Dimension
            {
                Id = dimDto.Id,
                Name = dimDto.Name,
                Type = dimDto.Type.ToInt()
            }).ToList();
            foreach (var dimDto in dimensionDtos)
            {
                if (dimDto.ParentDimension != null)
                {
                    var correspondingDimension = ret.Single(d => d.Name == dimDto.Name);
                    var correspondingParent = ret.Single(d => d.Name == dimDto.ParentDimension.Name);
                    correspondingDimension.ParentDimension = correspondingParent;
                }
            }
            return ret;
        }


    }
}
