using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender.Web.ViewModels
{

    public class MiningViewModel : SingleDatasetViewModelBase
    {
        public List<CommensurabilityViewModel> CommensurabilityList { get; set; }
        public string TaskName { get; set; }
        public double BaseQ { get; set; }
        public double Lift { get; set; }
        public bool ConditionRequired { get; set; }
    }
}