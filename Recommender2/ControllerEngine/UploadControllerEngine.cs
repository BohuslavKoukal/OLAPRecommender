using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.DTO.Mappers;
using Recommender.Business.FileHandling;
using Recommender.Business.FileHandling.Rdf;
using Recommender.Business.StarSchema;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class UploadControllerEngine : ControllerEngineBase
    {
        private readonly AttributeViewModelMapper _attributeMapper;
        private readonly StarSchemaBuilder _starSchemaBuilder;
        private readonly IFileHandler _fileHandler;
        private readonly DatasetViewModelMapper _datasetMapper;

        public UploadControllerEngine(IDataAccessLayer data, DatasetViewModelMapper datasetMapper,
            AttributeViewModelMapper attributeMapper, IFileHandler fileHandler,
            StarSchemaBuilder starSchemaBuilder, StarSchemaQuerier starSchemaQuerier) 
            : base(data, starSchemaQuerier)
        {
            _attributeMapper = attributeMapper;
            _starSchemaBuilder = starSchemaBuilder;
            _fileHandler = fileHandler;
            _datasetMapper = datasetMapper;
        }

        public SingleDatasetViewModel GetDataset(int id = 0)
        {
            return id == 0 ? new SingleDatasetViewModel() : _datasetMapper.Map(Data.GetDataset(id));
        }

        public SingleDatasetViewModel UploadFile(string datasetName, HttpPostedFileBase fileBase, string separator)
        {
            var file = _fileHandler.SaveFile(fileBase, separator);
            var dataset = new Dataset
            {
                State = State.FileUploaded,
                Name = datasetName,
                CsvFilePath = file.FilePath,
                Attributes = file.Attributes?.ToList()
            };
            Data.Insert(dataset);
            return _datasetMapper.Map(dataset);
        }

        public void CreateDataset(AttributeViewModel[] attributes, int id, string separator, string dateFormat)
        {
            var measures = _attributeMapper.MapToMeasures(attributes).ToList();
            var dimensions = _attributeMapper.MapToDimensions(attributes.ToList()).ToList();
            var columns = _attributeMapper.MapToDimensionsAndMeasures(attributes.ToList());
            var data = _fileHandler.GetValues(Data.GetDataset(id).CsvFilePath, columns.ToList(), separator, dateFormat);
            BuildStarSchema(id, data, dimensions, measures);
        }

        public void CreateDataset(int id, HttpPostedFileBase dsdFile)
        {
            var file = _fileHandler.SaveFile(dsdFile, string.Empty);
            var rdfLoader = new RdfLoader(file.FilePath, Data.GetCsvFilePath(id));
            // Check names for Sql safety
            var dimensionDtos = rdfLoader.GetDimensions().ToList();
            var measureDtos = rdfLoader.GetMeasures().ToList();
            var dimensions = DimensionMapper.ConvertToDimensions(dimensionDtos);
            var measures = measureDtos.Select(d => d.ConvertToMeasure()).ToList();
            var data = rdfLoader.ConvertObservationsToDataTable(dimensionDtos, measureDtos);
            BuildStarSchema(id, data, dimensions, measures);
        }

        private void BuildStarSchema(int id, DataTable data, List<Dimension> dimensions, List<Measure> measures)
        {
            PopulateDimensionsWithValues(dimensions, data);
            Data.PopulateDataset(id, measures, dimensions, State.DimensionsAndMeasuresSet);
            var dataset = Data.GetDataset(id);
            _starSchemaBuilder.CreateAndFillDimensionTables(dataset.Name, dataset.Dimensions.ToList(), data);
            _starSchemaBuilder.CreateFactTable(dataset.Name, dataset.Dimensions.ToList(), dataset.Measures.ToList());
            _starSchemaBuilder.FillFactTable(dataset.Name, dimensions, measures, data);
        }

        private void PopulateDimensionsWithValues(IEnumerable<Dimension> dimensions, DataTable data)
        {
            foreach (var dimension in dimensions)
            {
                dimension.DimensionValues = StarSchemaHelper.GetDimensionValues(dimension, data);
            }
        }


    }
}