using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Models;

namespace Recommender2.Business.DTO
{
    public class DimensionValue
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