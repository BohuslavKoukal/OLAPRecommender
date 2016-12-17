using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.Business.Enums;
using Recommender2.Business.Helpers;
using Recommender2.Models;

namespace Recommender2.ViewModels.Mappers
{
    public interface IDatasetViewModelMapper
    {
        DatasetViewModel Map(IEnumerable<Dataset> datasets);
        SingleDatasetViewModel Map(Dataset dataset);
    }

    public class DatasetViewModelMapper : IDatasetViewModelMapper
    {
        private readonly IDimensionTreeBuilder _treeBuilder;
        public DatasetViewModelMapper(IDimensionTreeBuilder treeBuilder)
        {
            _treeBuilder = treeBuilder;
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

        public SingleDatasetViewModel Map(Dataset dataset)
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
                    Type = ((DataType)measure.Type).ToString()
                }).ToList(),
                Dimensions = dataset.Dimensions?.Select(dimension => new DimensionViewModel
                {
                    Id = dimension.Id,
                    Name = dimension.Name,
                    Type = ((DataType)dimension.Type).ToString()
                }).ToList(),
                DimensionsSelectList = dimensionSelectList,
                CsvFilePath = dataset.CsvFilePath,
                State = dataset.State
            };
        }

        
    }
}