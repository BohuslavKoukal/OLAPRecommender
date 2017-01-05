using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Business.DTO.Mappers
{
    public static class MeasureMapper
    {
        public static Measure ConvertToMeasure(this MeasureDto measure)
        {
            return new Measure
            {
                Name = measure.Name,
                RdfUri = measure.RdfUri,
                Type = measure.Type.ToInt(),
                Id = measure.Id
            };
        }
    }
}
