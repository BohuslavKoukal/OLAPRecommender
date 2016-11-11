using System;
using System.Collections.Generic;
using System.Linq;
using Recommender2.Business;
using Recommender2.Business.Service;
using Recommender2.DataAccess;
using Recommender2.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class BrowseCubeControllerEngine : ControllerEngineBase
    {
        private readonly DatasetViewModelMapper _datasetMapper;
        private readonly BrowseCubeViewModelMapper _browseCubeMapper;
        private readonly GraphService _graphService;

        public BrowseCubeControllerEngine(DataAccessLayer data, DatasetViewModelMapper datasetMapper,
            BrowseCubeViewModelMapper browseCubeMapper,
            CsvHandler csvHandler, StarSchemaBuilder starSchemaBuilder, StarSchemaQuerier starSchemaQuerier, GraphService graphService) 
            : base(data, csvHandler, starSchemaBuilder, starSchemaQuerier)
        {
            _datasetMapper = datasetMapper;
            _browseCubeMapper = browseCubeMapper;
            _graphService = graphService;
        }

        public DatasetViewModel GetDatasets()
        {
            return _datasetMapper.Map(Data.GetAllDatasets());
        }
        public SingleDatasetViewModel GetDataset(int id)
        {
            return _datasetMapper.Map(Data.GetDataset(id));
        }

        public BrowseCubeViewModel BrowseCube(int id)
        {
            return _browseCubeMapper.Map(Data.GetDataset(id));
        }

        public BrowseCubeViewModel ShowChart(int id, int selectedMeasureId, int xDimensionId, int legendDimensionId)
        {
            var xDimension = Data.GetDimension(xDimensionId);
            var legendDimension = Data.GetDimension(legendDimensionId);
            var measure = Data.GetMeasure(selectedMeasureId);
            var groupedChart = GetGroupedChart(xDimension, legendDimension, measure);
            var drilldownChart = GetDrilldownChart(xDimension, measure);

            return new BrowseCubeViewModel
            {
                Dataset = _datasetMapper.Map(xDimension.DataSet),
                GroupedChart = groupedChart,
                LegendDimensionId = legendDimensionId,
                SelectedMeasureId = selectedMeasureId,
                XDimensionId = xDimensionId,
                MeasureName = measure.Name,
                ShouldChartBeDisplayed = true,
                DrilldownChart = drilldownChart
            };
        }

        private GroupedChartViewModel GetGroupedChart(Dimension xDimension, Dimension legendDimension, Measure measure)
        {
            var groupedGraphDto = _graphService.GetGroupedGraph(xDimension, legendDimension, measure);
            return _browseCubeMapper.Map(groupedGraphDto);
        }

        private DrilldownChartViewModel GetDrilldownChart(Dimension xDimension, Measure measure)
        {
            var drilldownGraphDto = _graphService.GetDrilldownGraph(xDimension, measure);
            return _browseCubeMapper.Map(drilldownGraphDto);
        }

        public byte[] GetFile(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }
    }
}