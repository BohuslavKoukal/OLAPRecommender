using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Business.DTO;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Web.ViewModels.Mappers
{
    public interface IBrowseCubeViewModelMapper
    {
        BrowseCubeViewModel Map(Dataset dataset, FilterViewModel filter);
        GroupedChartViewModel Map(GroupedGraphDto graph);
        DrilldownChartViewModel Map(DrilldownGraphDto graph);
        FilterViewModel Map(DimensionTree tree);
    }

    public class BrowseCubeViewModelMapper : IBrowseCubeViewModelMapper
    {
        private readonly DatasetViewModelMapper _datasetMapper;

        public BrowseCubeViewModelMapper(DatasetViewModelMapper datasetMapper)
        {
            _datasetMapper = datasetMapper;
        }

        public BrowseCubeViewModel Map(Dataset dataset, FilterViewModel filter)
        {
            return new BrowseCubeViewModel
            {
                Dataset = _datasetMapper.Map(dataset, filter)
            };
        }

        public GroupedChartViewModel Map(GroupedGraphDto graph)
        {
            var viewModel = new GroupedChartViewModel
            {
                Categories = new List<GroupedCategoryViewModel>(),
                ChartTitle = graph.Name,
                Series = new List<GroupedSeriesViewModel>()
            };
            viewModel.Categories.AddRange(GetCategories(graph.Roots));
            viewModel.Series.AddRange(GetSeries(graph.GetLegendValues().ToList()));
            return viewModel;
        }

        public DrilldownChartViewModel Map(DrilldownGraphDto graph)
        {
            if (graph == null) return null;
            return new DrilldownChartViewModel
            {
                ChartTitle = graph.Name,
                Series = new [] { GetSeries(graph) } ,
                Drilldown = GetDrilldown(graph)
            };
        }

        public FilterViewModel Map(DimensionTree tree)
        {
            var viewModel = new FilterViewModel { Dimensions = new List<FilterDimensionViewModel>() };
            var dimensionDtos = tree.GetDimensionDtos().ToList();
            foreach (var dimensionDto in dimensionDtos)
            {
                var dimensionViewModel = new FilterDimensionViewModel
                {
                    DimensionId = dimensionDto.Id,
                    DimensionName = dimensionDto.Name,
                    Values = new List<DimensionValueViewModel>()
                };
                dimensionViewModel.Values.AddRange(dimensionDto.DimensionValues
                    .Select(v => new DimensionValueViewModel {Id = v.Id, Value = v.Value, Checked = true}));
                viewModel.Dimensions.Add(dimensionViewModel);
            }
            return viewModel;
        }

        
        private DrilldownSeriesViewModel GetSeries(DrilldownGraphDto graph)
        {
            var graphRoots = graph.Roots.Cast<DrilldownGraphXAxisRootDto>().ToList();
            var series = new DrilldownSeriesViewModel
            {
                Id = graph.Name,
                Name = graph.DimensionName,
                Data = new DrilldownSeriesDataViewModel[graph.Roots.Count]
            };
            for (int i = 0; i < graph.Roots.Count; i++)
            {
                series.Data[i] = new DrilldownSeriesDataViewModel
                {
                    Drilldown = graphRoots[i].Name,
                    Name = graphRoots[i].Name,
                    Y = graphRoots[i].GetValue()
                };
            }
            return series;
        }
        
        private DrilldownSeriesViewModel GetSeries(DrilldownGraphXAxisRootDto root)
        {
            var rootXAxisLeaves = root.XAxisLeaves.Cast<DrilldownGraphXAxisLeafDto>().ToList();
            var series = new DrilldownSeriesViewModel
            {
                Id = root.Name,
                Name = root.Name,
                Data = new DrilldownSeriesDataViewModel[rootXAxisLeaves.Count]
            };
            for (int i = 0; i < rootXAxisLeaves.Count; i++)
            {
                series.Data[i] = new DrilldownSeriesDataViewModel
                {
                    Drilldown = null,
                    Name = rootXAxisLeaves[i].Name,
                    Y = rootXAxisLeaves[i].Value
                };
            }
            return series;
        }

        private DrilldownViewModel GetDrilldown(DrilldownGraphDto graph)
        {
            var viewModel = new DrilldownViewModel
            {
                Series = new DrilldownSeriesViewModel[graph.Roots.Count]
            };
            for (int i = 0; i < graph.Roots.Count; i++)
            {
                viewModel.Series[i] = GetSeries(graph.Roots.Cast<DrilldownGraphXAxisRootDto>().ToList()[i]);
            }
            return viewModel;
        }

        private IEnumerable<GroupedSeriesViewModel> GetSeries(List<GraphLegendValueDto> legendValues)
        {
            // get distinct legends
            var legends = legendValues.
                Select(legendValue => legendValue.Legend)
                .GroupBy(legend => legend.Id)
                .Select(group => group.First());
            foreach (var legend in legends)
            {
                var thisLegendValues = legendValues.Where(lv => lv.Legend.Id == legend.Id);
                yield return new GroupedSeriesViewModel
                {
                    Name = legend.Name,
                    Data = thisLegendValues.Select(lv => lv.Value).ToArray()
                };
            }
        }

        private IEnumerable<GroupedCategoryViewModel> GetCategories(IEnumerable<GraphXAxisRootDto> graphRoots)
        {
            var categories = new List<GroupedCategoryViewModel>();
            foreach (var graphRoot in graphRoots)
            {
                categories.AddRange(GetCategories(graphRoot));
            }
            return categories;
        }

        private IEnumerable<GroupedCategoryViewModel> GetCategories(GraphXAxisRootDto graphRoot)
        {
            if (string.IsNullOrEmpty(graphRoot.Name))
            {
                return graphRoot.XAxisLeaves.Select(l => new GroupedCategoryViewModel
                {
                    Name = l.Name
                });

            }
            else
            {
                return new[] {new GroupedCategoryViewModel
                    {
                        Name = graphRoot.Name,
                        Categories =
                            new List<GroupedCategoryViewModel>(graphRoot.XAxisLeaves.Select(l => new GroupedCategoryViewModel
                            {
                                Name = l.Name
                            }))
                    }};
            }
        }


    }
}