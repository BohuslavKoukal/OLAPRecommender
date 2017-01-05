using System.Collections.Generic;
using Recommender.Data.Models;

namespace Recommender.Business.DTO
{
    public class DimensionValueDto
    {
        public DimensionDto Dimension { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
        public string RdfUri { get; set; }
        public DimensionValueDto ParentDimensionValue { get; set; }
    }

    public class DimensionValueIds
    {
        public int DimensionId { get; set; }
        public HashSet<int> ValueIds { get; set; }
    }
}