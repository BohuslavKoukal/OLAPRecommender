using System;
using System.Collections.Generic;
using System.Linq;
using Recommender.Business;
using Recommender.Business.DTO;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class BrowseCubeControllerEngine : ControllerEngineBase
    {
        private readonly IBrowseCubeViewModelMapper _browseCubeMapper;
        private readonly IGraphService _graphService;
        private readonly IDimensionTreeBuilder _treeBuilder;
        private readonly SubcubeService _subcubeService;
        private readonly IDatasetViewModelMapper _datasetMapper;

        public BrowseCubeControllerEngine(IDataAccessLayer data, IDatasetViewModelMapper datasetMapper,
            IBrowseCubeViewModelMapper browseCubeMapper, SubcubeService subcubeService,
            IStarSchemaQuerier starSchemaQuerier, IGraphService graphService,
            IDimensionTreeBuilder treeBuilder) 
            : base(data, starSchemaQuerier)
        {
            _browseCubeMapper = browseCubeMapper;
            _graphService = graphService;
            _treeBuilder = treeBuilder;
            _subcubeService = subcubeService;
            _datasetMapper = datasetMapper;
        }

        
        public SingleDatasetViewModel GetDataset(int id)
        {
            var dataset = Data.GetDataset(id);
            var dimensionTree = _treeBuilder.ConvertToTree(id, true);
            return _datasetMapper.Map(dataset, GetFilterValues(dimensionTree));
        }

        public DatasetViewModel GetDatasets()
        {
            return _datasetMapper.Map(Data.GetAllDatasets());
        }

        public BrowseCubeViewModel BrowseCube(int id)
        {
            var dataset = Data.GetDataset(id);
            return _browseCubeMapper.Map(dataset, GetFilterValues(id));
        }

        public BrowseCubeViewModel ShowChart(int id, int selectedMeasureId, int xDimensionId, int legendDimensionId, bool group,
            Dictionary <int, Dictionary<int, bool>> filterDimensions)
        {
            var dataset = Data.GetDataset(id);
            var measure = Data.GetMeasure(selectedMeasureId);
            var filters = _subcubeService.GetFilters(filterDimensions);
            var dimensionTree = _treeBuilder.ConvertToTree(id, true);
            var groupedChart = GetGroupedChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId),
                dimensionTree.GetDimensionDto(legendDimensionId), measure, filters, group);
            var drilldownChart = GetDrilldownChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId), measure, filters);
            var filterValues = GetFilterValues(id);

            return new BrowseCubeViewModel
            {
                Dataset = _datasetMapper.Map(dataset, filterValues),
                GroupedChart = groupedChart,
                LegendDimensionId = legendDimensionId,
                SelectedMeasureId = selectedMeasureId,
                XDimensionId = xDimensionId,
                MeasureName = measure?.Name,
                ShouldChartBeDisplayed = true,
                DrilldownChart = drilldownChart
            };
        }

        private FilterViewModel GetFilterValues(DimensionTree tree)
        {
            return _browseCubeMapper.Map(tree);
        }

        private FilterViewModel GetFilterValues(int id)
        {
            var dimensionTree = _treeBuilder.ConvertToTree(id, true);
            return GetFilterValues(dimensionTree);
        }

        private GroupedChartViewModel GetGroupedChart(DimensionTree tree, TreeDimensionDto xDimension, DimensionDto legendDimension,
            Measure measure, List<FlatDimensionDto> filters, bool group)
        {
            var groupedGraphDto = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters, group);
            return _browseCubeMapper.Map(groupedGraphDto);
        }

        private DrilldownChartViewModel GetDrilldownChart(DimensionTree tree, TreeDimensionDto xDimension, Measure measure, List<FlatDimensionDto> filters)
        {
            var drilldownGraphDto = _graphService.GetDrilldownGraph(tree, xDimension, measure, filters);
            return _browseCubeMapper.Map(drilldownGraphDto);
        }

        public byte[] GetFile(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }
    }
}