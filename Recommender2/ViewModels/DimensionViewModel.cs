using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recommender2.ViewModels
{
	public class DimensionViewModel
	{
        [Display(Name = "Dimension Name")]
        public string Name { get; set; }

        [Display(Name = "Dimension Id")]
        public int Id { get; set; }

        [Display(Name = "Dimension Type")]
        public string Type { get; set; }
    }
}