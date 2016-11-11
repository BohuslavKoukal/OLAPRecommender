using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business;
using Recommender2.DataAccess;
using Recommender2.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class UploadControllerEngine : ControllerEngineBase
    {
        private readonly DatasetViewModelMapper _datasetMapper;
        private readonly AttributeViewModelMapper _attributeMapper;

        public UploadControllerEngine(DataAccessLayer data, DatasetViewModelMapper datasetMapper,
            AttributeViewModelMapper attributeMapper, CsvHandler csvHandler,
            StarSchemaBuilder starSchemaBuilder, StarSchemaQuerier starSchemaQuerier) 
            : base(data, csvHandler, starSchemaBuilder, starSchemaQuerier)
        {
            _datasetMapper = datasetMapper;
            _attributeMapper = attributeMapper;
        }

        public SingleDatasetViewModel UploadFile(string datasetName, HttpPostedFileBase fileBase)
        {
            var file = CsvHandler.GetFile(fileBase);
            var dataset = new Dataset
            {
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
            Data.PopulateDataset(id, measures, dimensions);
            var dataset = Data.GetDataset(id);
            var data = CsvHandler.GetValues(dataset.CsvFilePath);
            StarSchemaBuilder.CreateAndFillDimensionTables(dataset.Name, dimensions, data);
            StarSchemaBuilder.CreateFactTable(dataset.Name, dataset.Dimensions.ToList(), dataset.Measures.ToList());
            StarSchemaBuilder.FillFactTable(dataset.Name, dimensions, measures, data);
        }


    }
}