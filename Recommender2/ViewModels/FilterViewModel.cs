using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business.DTO;

namespace Recommender2.ViewModels
{
    public class FilterViewModel
    {
        public List<FilterDimensionViewModel> Dimensions { get; set; }
    }

    public class FilterDimensionViewModel
    {
        public int DimensionId { get; set; }
        public string DimensionName { get; set; }
        public List<DimensionValueViewModel> Values { get; set; }
    }

    public class DimensionValueViewModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
    }
}