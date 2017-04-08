using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;

namespace Recommender.Web.ControllerEngine
{
    public class BrowseCubeControllerEngine : ControllerEngineBase
    {
        private readonly IBrowseCubeViewModelMapper _browseCubeMapper;
        private readonly IGraphService _graphService;
        private readonly IDimensionTreeBuilder _treeBuilder;
        //private readonly SubcubeService _subcubeService;
        private readonly IDatasetViewModelMapper _datasetMapper;
        private readonly AssociationRuleToViewMapper _ruleToViewMapper;

        public BrowseCubeControllerEngine(IDataAccessLayer data, IDatasetViewModelMapper datasetMapper,
            IBrowseCubeViewModelMapper browseCubeMapper,
            IStarSchemaQuerier starSchemaQuerier, IGraphService graphService,
            IDimensionTreeBuilder treeBuilder, AssociationRuleToViewMapper ruleToViewMapper) 
            : base(data, starSchemaQuerier)
        {
            _browseCubeMapper = browseCubeMapper;
            _graphService = graphService;
            _treeBuilder = treeBuilder;
            //_subcubeService = subcubeService;
            _datasetMapper = datasetMapper;
            _ruleToViewMapper = ruleToViewMapper;
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

        public BrowseCubeViewModel ShowChart(int ruleId)
        {
            var rule = Data.GetRule(ruleId);
            var datasetId = rule.MiningTask.DataSet.Id;
            var measureId = rule.Succedents.First().Measure.Id;
            var xAndLegendIds = _ruleToViewMapper.GetXAndLegendDimensionsId(rule, _treeBuilder.ConvertToTree(datasetId));
            var filters = _ruleToViewMapper.GetFilterValues(rule);
            var chartText = _ruleToViewMapper.GetChartText(rule);
            return ShowChart(datasetId, measureId, xAndLegendIds.Item1, xAndLegendIds.Item2, false, filters.ToList(), chartText, true);
        }

        public BrowseCubeViewModel ShowChart(int id, int selectedMeasureId, int xDimensionId, int legendDimensionId,
            bool group, FilterViewModel filterDimensions)
        {
            return ShowChart(id, selectedMeasureId, xDimensionId, legendDimensionId, group, GetFilters(filterDimensions), String.Empty, false);
        }

        public BrowseCubeViewModel ShowChart(int id, int selectedMeasureId, int xDimensionId, int legendDimensionId, bool group,
            List<FlatDimensionDto> filters, string chartText, bool requireDrilldownChart)
        {
            var dataset = Data.GetDataset(id);
            var measure = Data.GetMeasure(selectedMeasureId);
            var dimensionTree = _treeBuilder.ConvertToTree(id, true);
            GroupedChartViewModel groupedChart = null;
            if (legendDimensionId != 0)
            {
                groupedChart = GetGroupedChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId),
                dimensionTree.GetDimensionDto(legendDimensionId), measure, filters, group);
            }
            var drilldownChart = GetDrilldownChart(dimensionTree, dimensionTree.GetDimensionDto(xDimensionId), measure, filters, requireDrilldownChart);
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
                DrilldownChart = drilldownChart,
                ChartText = chartText
            };
        }

        private List<FlatDimensionDto> GetFilters(FilterViewModel filter)
        {
            var ret = new List<FlatDimensionDto>();
            // do not filter dimensions where all values are true
            foreach (var dim in filter.Dimensions)
            {
                var dimension = Data.GetDimension(dim.DimensionId);
                var dimensionDto = new FlatDimensionDto
                {
                    Id = dimension.Id,
                    Name = dimension.Name,
                    DimensionValues = new List<DimensionValueDto>()
                };
                // do not include dimensions with all values checked
                if (!dim.Values.Select(v => v.Checked).Contains(false)) continue;
                foreach (var value in dim.Values)
                {
                    if (value.Checked)
                        dimensionDto.DimensionValues.Add(new DimensionValueDto { Id = value.Id });
                }
                ret.Add(dimensionDto);
            }
            return ret;
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

        private DrilldownChartViewModel GetDrilldownChart(DimensionTree tree, TreeDimensionDto xDimension, Measure measure,
            List<FlatDimensionDto> filters, bool requireDrilldownChart)
        {
            var drilldownGraphDto = _graphService.GetDrilldownGraph(tree, xDimension, measure, filters, requireDrilldownChart);
            return _browseCubeMapper.Map(drilldownGraphDto);
        }

        public byte[] GetFile(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }
    }
}