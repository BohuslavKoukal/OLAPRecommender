using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class UploadControllerEngine : ControllerEngineBase
    {
        private readonly DatasetViewModelMapper _datasetMapper;
        private readonly AttributeViewModelMapper _attributeMapper;
        private readonly StarSchemaBuilder _starSchemaBuilder;
        private readonly CsvHandler _csvHandler;

        public UploadControllerEngine(IDataDecorator data, DatasetViewModelMapper datasetMapper,
            AttributeViewModelMapper attributeMapper, CsvHandler csvHandler,
            StarSchemaBuilder starSchemaBuilder, StarSchemaQuerier starSchemaQuerier) 
            : base(data, starSchemaQuerier)
        {
            _datasetMapper = datasetMapper;
            _attributeMapper = attributeMapper;
            _starSchemaBuilder = starSchemaBuilder;
            _csvHandler = csvHandler;
        }

        public SingleDatasetViewModel GetDataset(int id = 0)
        {
            return id == 0 ? new SingleDatasetViewModel() : _datasetMapper.Map(Data.GetDataset(id));
        }

        public SingleDatasetViewModel UploadFile(string datasetName, HttpPostedFileBase fileBase, string separator)
        {
            var file = _csvHandler.GetFile(fileBase, separator);
            var dataset = new DatasetDto
            {
                State = State.FileUploaded,
                Name = datasetName,
                CsvFilePath = file.FilePath,
                Attributes = file.Attributes
            };
            Data.Insert(dataset);
            return _datasetMapper.Map(dataset);
        }

        public void CreateDataset(int id, AttributeViewModel[] attributes)
        {
            var measures = _attributeMapper.MapToMeasures(attributes).ToList();
            var dimensions = _attributeMapper.MapToDimensions(attributes.ToList()).ToList();
            Data.PopulateDataset(id, measures, dimensions, State.DimensionsAndMeasuresSet);
            var dataset = Data.GetDataset(id);
            var data = _csvHandler.GetValues(dataset.CsvFilePath);
            _starSchemaBuilder.CreateAndFillDimensionTables(dataset.Name, dimensions, data);
            _starSchemaBuilder.CreateFactTable(dataset.Name, dataset.Dimensions.ToList(), dataset.Measures.ToList());
            _starSchemaBuilder.FillFactTable(dataset.Name, dimensions, measures, data);
        }


    }
}