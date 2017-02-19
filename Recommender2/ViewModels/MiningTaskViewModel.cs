using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recommender.Common.Enums;

namespace Recommender2.ViewModels
{
    public class MiningTaskViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Task Name")]
        public string Name { get; set; }
        
        public List<AssociationRuleViewModel> AssociationRules { get; set; }

        public int NumberOfVerifications { get; set; }
        public TaskState TaskState { get; set; }
        public double Base { get; set; }
        public double Aad { get; set; }
        public DateTime TaskStartTime { get; set; }
        public TimeSpan TaskDuration { get; set; }
        public FilterViewModel Filters { get; set; }
    }
}