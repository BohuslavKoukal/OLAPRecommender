using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recommender2.ViewModels
{
    public class MeasureViewModel
    {
        [Display(Name = "Measure Name")]
        public string Name { get; set; }

        [Display(Name = "Measure Id")]
        public int Id { get; set; }

        [Display(Name = "Measure Type")]
        public string Type { get; set; }
    }
}