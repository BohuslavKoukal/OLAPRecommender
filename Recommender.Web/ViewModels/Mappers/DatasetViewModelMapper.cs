using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender.Business;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender.Web.ViewModels.Mappers
{
    public interface IDatasetViewModelMapper
    {
        DatasetViewModel Map(IEnumerable<Dataset> datasets);
        SingleDatasetViewModel Map(Dataset dataset, FilterViewModel filterValues = null);
    }

    public class DatasetViewModelMapper : IDatasetViewModelMapper
    {
        private readonly IDimensionTreeBuilder _treeBuilder;
        private readonly IMiningTaskViewModelMapper _taskMapper;
        public DatasetViewModelMapper(IDimensionTreeBuilder treeBuilder, IMiningTaskViewModelMapper taskMapper)
        {
            _treeBuilder = treeBuilder;
            _taskMapper = taskMapper;
        }

        public DatasetViewModel Map(IEnumerable<Dataset> datasets)
        {
            var datasetViewModel = new DatasetViewModel();
            foreach (var dataset in datasets)
            {
                datasetViewModel.Datasets.Add(Map(dataset));
            }
            return datasetViewModel;
        }

        public SingleDatasetViewModel Map(Dataset dataset, FilterViewModel filterValues = null)
        {
            List<SelectListItem> dimensionSelectList;
            if (dataset.State >= State.DimensionsAndMeasuresSet)
            {
                var dimensionTree = _treeBuilder.ConvertToTree(dataset.Dimensions?.ToList());
                dimensionSelectList = _treeBuilder.ConvertTreeToSelectList(dimensionTree);
            }
            else
            {
                dimensionSelectList = new List<SelectListItem>();
            }
            return new SingleDatasetViewModel
            {
                Id = dataset.Id,
                Name = dataset.Name,
                Attributes = dataset.Attributes?.Select(attribute => new AttributeViewModel
                {
                    Name = attribute.Name
                }).ToList(),
                Measures = dataset.Measures?.Select(measure => new MeasureViewModel
                {
                    Id = measure.Id,
                    Name = measure.Name,
                    Type = measure.Type.ToString()
                }).ToList(),
                Dimensions = dataset.Dimensions?.Select(dimension => new DimensionViewModel
                {
                    Id = dimension.Id,
                    Name = dimension.Name,
                    Type = dimension.Type.ToString()
                }).ToList(),
                MiningTasks = dataset.MiningTasks?.Select(task => _taskMapper.Map(task)).ToList(),
                Filter = filterValues,
                DimensionsSelectList = dimensionSelectList,
                FilePath = dataset.CsvFilePath,
                State = dataset.State
            };
        }

        
    }
}