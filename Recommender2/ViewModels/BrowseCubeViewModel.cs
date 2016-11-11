using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.ViewModels
{
    public class BrowseCubeViewModel
    {
        public SingleDatasetViewModel Dataset { get; set; }
        public int DatasetId => Dataset.Id;

        public int SelectedMeasureId { get; set; }
        public int XDimensionId { get; set; }
        public int LegendDimensionId { get; set; }

        public GroupedChartViewModel GroupedChart { get; set; }
        public DrilldownChartViewModel DrilldownChart { get; set; }

        public string MeasureName { get; set; }

        public bool ShouldChartBeDisplayed { get; set; }
    }

    public class GroupedChartViewModel : BaseChartViewModel
    {
        public List<GroupedSeriesViewModel> Series { get; set; }
        public List<GroupedCategoryViewModel> Categories { get; set; }
    }

    public class DrilldownChartViewModel : BaseChartViewModel
    {
        public DrilldownSeriesViewModel[] Series { get; set; }
        public DrilldownViewModel Drilldown { get; set; }
    }

    public class BaseChartViewModel
    {
        public string ChartTitle { get; set; }
        
    }
}