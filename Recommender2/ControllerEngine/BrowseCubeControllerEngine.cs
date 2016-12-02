using System;
using System.Collections.Generic;
using System.Linq;
using Recommender2.Business;
using Recommender2.Business.DTO;
using Recommender2.Business.Helpers;
using Recommender2.Business.Service;
using Recommender2.DataAccess;
using Recommender2.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class BrowseCubeControllerEngine : ControllerEngineBase
    {
        private readonly IDatasetViewModelMapper _datasetMapper;
        private readonly IBrowseCubeViewModelMapper _browseCubeMapper;
        private readonly IGraphService _graphService;
        private readonly IDimensionTreeBuilder _treeBuilder;

        public BrowseCubeControllerEngine(IDataAccessLayer data, IDatasetViewModelMapper datasetMapper,
            IBrowseCubeViewModelMapper browseCubeMapper,
            IStarSchemaQuerier starSchemaQuerier, IGraphService graphService,
            IDimensionTreeBuilder treeBuilder) 
            : base(data, starSchemaQuerier)
        {
            _datasetMapper = datasetMapper;
            _browseCubeMapper = browseCubeMapper;
            _graphService = graphService;
            _treeBuilder = treeBuilder;
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
            var dataset = Data.GetDataset(id);
            return _browseCubeMapper.Map(dataset, GetFilterValues(id));
        }

        public BrowseCubeViewModel ShowChart(int id, int selectedMeasureId, int xDimensionId, int legendDimensionId,
            Dictionary <int, Dictionary<int, bool>> filterDimensions)
        {
            var dataset = Data.GetDataset(id);
            var measure = Data.GetMeasure(selectedMeasureId);
            var filters = GetFilters(filterDimensions);
            var dimensionTree = _treeBuilder.ConvertToTree(id, true);
            var groupedChart = GetGroupedChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId),
                dimensionTree.GetDimensionDto(legendDimensionId), measure, filters);
            var drilldownChart = GetDrilldownChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId), measure, filters);
            var filterValues = GetFilterValues(id);

            return new BrowseCubeViewModel
            {
                Dataset = _datasetMapper.Map(dataset),
                GroupedChart = groupedChart,
                LegendDimensionId = legendDimensionId,
                SelectedMeasureId = selectedMeasureId,
                XDimensionId = xDimensionId,
                MeasureName = measure?.Name,
                ShouldChartBeDisplayed = true,
                DrilldownChart = drilldownChart,
                Filter = filterValues
            };
        }

        private FilterViewModel GetFilterValues(int datasetId)
        {
            var tree = _treeBuilder.ConvertToTree(datasetId, true);
            return _browseCubeMapper.Map(tree);
        }

        private List<FlatDimensionDto> GetFilters(Dictionary<int, Dictionary<int, bool>> filterDimensions)
        {
            var ret = new List<FlatDimensionDto>();
            // do not filter dimensions where all values are true
            foreach (var fd in filterDimensions)
            {
                var dimension = Data.GetDimension(fd.Key);
                var dimensionDto = new FlatDimensionDto
                    {
                        Id = dimension.Id,
                        Name = dimension.Name,
                        DimensionValues = new List<DimensionValue>()
                    };
                // do not include dimensions with all values checked
                if (!fd.Value.Select(v => v.Value).Contains(false)) continue;
                foreach (var value in fd.Value)
                {
                    if (value.Value)
                        dimensionDto.DimensionValues.Add(new DimensionValue { Id = value.Key });
                }
                ret.Add(dimensionDto);
            }
            return ret;
        }

        private GroupedChartViewModel GetGroupedChart(DimensionTree tree, TreeDimensionDto xDimension, DimensionDto legendDimension,
            Measure measure, List<FlatDimensionDto> filters)
        {
            var groupedGraphDto = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters);
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