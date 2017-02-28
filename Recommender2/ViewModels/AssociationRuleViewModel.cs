using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recommender2.ViewModels
{
    public class AssociationRuleViewModel
    {
        //[Display(Name = "Association Rule Text")]
        //public string AssociationRuleText { get; set; }

        public List<string> AntecedentValues { get; set; }
        public List<string> ConditionValues { get; set; }
        public List<SuccedentViewModel> Succedents { get; set; }

        public int Id { get; set; }
    }

    public class SuccedentViewModel
    {
        public string Text { get; set; }
        public double Aad { get; set; }
        public double Base { get; set; }
    }
}