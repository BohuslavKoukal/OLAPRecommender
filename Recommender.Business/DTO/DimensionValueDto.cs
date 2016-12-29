using System.Collections.Generic;

namespace Recommender.Business.DTO
{
    public class DimensionValueDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class DimensionValueIds
    {
        public int DimensionId { get; set; }
        public HashSet<int> ValueIds { get; set; }
    }
}