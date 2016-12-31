using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Recommender2.ViewModels
{
    public class AttributeViewModel
    {
        [Required]
        [Display(Name = "Attribute Name")]
        public string Name { get; set; }

        [Display(Name = "Attribute Type")]
        public string SelectedAttributeType { get; set; }

        [Display(Name = "Attribute Role")]
        public int SelectedAttributeRoleId { get; set; }

        [Display(Name = "Parent dimension")]
        public string SelectedAttributeParentName { get; set; }
        
    }
}