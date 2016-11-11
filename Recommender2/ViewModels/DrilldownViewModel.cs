using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.ViewModels
{
    public class DrilldownViewModel
    {
        public DrilldownSeriesViewModel[] Series { get; set; }
    }

    public class DrilldownSeriesViewModel
    {
        public string Id { get; set; }
        // Category
        public string Name { get; set; }
        public DrilldownSeriesDataViewModel[] Data { get; set; }
    }

    public class DrilldownSeriesDataViewModel
    {
        // Diary
        public string Name { get; set; }
        // Diary
        public string Drilldown { get; set; }
        // Cake + bread + bun
        public double Y { get; set; }
    }

}