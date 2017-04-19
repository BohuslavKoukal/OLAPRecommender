using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.FileHandling;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender.Web.ControllerEngine;
using Recommender.Web.Controllers;
using Recommender.Web.Validations;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;
using RecommenderTests.Helpers;

namespace RecommenderTests.WebTests
{
    [TestClass]
    public class ControllerTests
    {
        private readonly HomeController _home;
        private readonly BrowseCubeController _bc;
        private readonly BrowseCubeControllerEngine _bce;
        private readonly MinedResultsController _mr;
        private readonly MinedResultsControllerEngine _mre;
        private readonly UploadController _uc;
        private readonly UploadControllerEngine _uce;

        private readonly Mock<IDataAccessLayer> _data;
        private readonly Mock<IDatasetViewModelMapper> _datasetMapper;
        private readonly Mock<IBrowseCubeViewModelMapper> _browseCubeMapper;
        private readonly Mock<AttributeViewModelMapper> _attributeMapper;
        private readonly Mock<IMiningTaskViewModelMapper> _taskMapper;
        private readonly Mock<IStarSchemaQuerier> _querier;
        private readonly Mock<IGraphService> _graphService;
        private readonly Mock<IDimensionTreeBuilder> _treeBuilder;
        private readonly Mock<IAssociationRuleToViewMapper> _arMapper;
        private readonly Mock<IAssociationRulesTaskProcessor> _processor;
        private readonly Mock<IDataDiscretizator> _discretizator;
        private readonly Mock<IInputValidations> _validations;
        private readonly Mock<IFileHandler> _handler;
        private readonly Mock<IStarSchemaBuilder> _builder;

        public ControllerTests()
        {
            _data = new Mock<IDataAccessLayer>();
            _attributeMapper = new Mock<AttributeViewModelMapper>();
            _datasetMapper = new Mock<IDatasetViewModelMapper>();
            _browseCubeMapper = new Mock<IBrowseCubeViewModelMapper>();
            _querier = new Mock<IStarSchemaQuerier>();
            _graphService = new Mock<IGraphService>();
            _treeBuilder = new Mock<IDimensionTreeBuilder>();
            _arMapper = new Mock<IAssociationRuleToViewMapper>();
            _processor = new Mock<IAssociationRulesTaskProcessor>();
            _discretizator = new Mock<IDataDiscretizator>();
            _taskMapper = new Mock<IMiningTaskViewModelMapper>();
            _validations = new Mock<IInputValidations>();
            _handler = new Mock<IFileHandler>();
            _builder = new Mock<IStarSchemaBuilder>();

            _home = new HomeController();

            _bce = new BrowseCubeControllerEngine(_data.Object, _datasetMapper.Object, _browseCubeMapper.Object,
                _querier.Object, _graphService.Object, _treeBuilder.Object, _arMapper.Object);
            _bc = new BrowseCubeController(_bce);

            _mre = new MinedResultsControllerEngine(_data.Object, _querier.Object, _processor.Object, _taskMapper.Object, _discretizator.Object, _treeBuilder.Object);
            _mr = new MinedResultsController(_mre, _bce, _validations.Object);

            _uce = new UploadControllerEngine(_data.Object, _datasetMapper.Object, _attributeMapper.Object, _handler.Object, _builder.Object, _querier.Object);
            _uc = new UploadController(_uce, _validations.Object);
        }

        private void Reset()
        {
            _data.Reset();
            _datasetMapper.Reset();
            _browseCubeMapper.Reset();
            _attributeMapper.Reset();
            _taskMapper.Reset();
            _querier.Reset();
            _graphService.Reset();
            _treeBuilder.Reset();
            _arMapper.Reset();
            _processor.Reset();
            _discretizator.Reset();
            _validations.Reset();
            _handler.Reset();
            _builder.Reset();
    }

        [TestMethod]
        public void TestHomeIndex()
        {
            // Act
            var result = _home.Index() as ViewResult;
            // Assert
            result.Should().NotBe(null);
        }

        [TestMethod]
        public void TestHomeAbout()
        {
            // Act
            var result = _home.About() as ViewResult;
            // Assert
            result.Should().NotBe(null);
        }

        [TestMethod]
        public void TestHomeContact()
        {
            // Act
            var result = _home.Contact() as ViewResult;
            // Assert
            result.Should().NotBe(null);
        }

        [TestMethod]
        public void TestBrowseCubeIndex()
        {
            // Arrange
            Reset();
            // Act
            var result = _bc.Index() as ViewResult;
            // Assert
            _data.Verify(d => d.GetAllDatasets(), Times.Once);
            _datasetMapper.Verify(d => d.Map(It.IsAny<IEnumerable<Dataset>>()), Times.Once);
        }

        [TestMethod]
        public void TestBrowseCubeDetails()
        {
            // Arrange
            Reset();
            // Act
            var result = _bc.Details(1) as ViewResult;
            // Assert
            _data.Verify(d => d.GetDataset(1), Times.Once);
            _treeBuilder.Verify(tb => tb.ConvertToTree(1, true), Times.Once);
            _browseCubeMapper.Verify(d => d.Map(It.IsAny<Dataset>(), It.IsAny<FilterViewModel>()), Times.Once);
        }

        [TestMethod]
        public void TestBrowseCubeShowRule()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            var dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            var measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var task = BusinessLayerTestHelper.GetTask(dataset);
            var rule = BusinessLayerTestHelper.GetAssociationRule(dimensions, measures, task);
            _data.Setup(d => d.GetRule(It.IsAny<int>())).Returns(rule);
            _arMapper.Setup(ar => ar.GetXAndLegendDimensionsId(It.IsAny<AssociationRule>(), It.IsAny<DimensionTree>()))
                .Returns(Tuple.Create(1, 1));
            var tree = BusinessLayerTestHelper.CreateDimensionTree(BusinessLayerTestHelper.DatasetName);
            _treeBuilder.Setup(tb => tb.ConvertToTree(It.IsAny<int>(), It.IsAny<bool>())).Returns(tree);
            // Act
            var result = _bc.ShowChart(1);
            // Assert
            _data.Verify(d => d.GetRule(1), Times.Once);
            _treeBuilder.Verify(tb => tb.ConvertToTree(It.IsAny<int>(), It.IsAny<bool>()));
            _arMapper.Verify(ar => ar.GetXAndLegendDimensionsId(It.IsAny<AssociationRule>(), It.IsAny<DimensionTree>()));
            _arMapper.Verify(ar => ar.GetFilterValues(It.IsAny<AssociationRule>()));
            _arMapper.Verify(ar => ar.GetChartText(It.IsAny<AssociationRule>()));
            ShowChartCalled();
        }

        public void ShowChartCalled()
        {
            _data.Verify(d => d.GetDataset(It.IsAny<int>()));
            _data.Verify(d => d.GetMeasure(It.IsAny<int>()));
            _treeBuilder.Verify(tb => tb.ConvertToTree(It.IsAny<int>(), It.IsAny<bool>()));
        }

        [TestMethod]
        public void TestUploadUploadFile()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            _data.Setup(d => d.GetDataset(It.IsAny<int>())).Returns(dataset);
            _validations.Setup(
                v =>
                    v.DatatypesAreValid(It.IsAny<AttributeViewModel[]>(), It.IsAny<int>(), It.IsAny<string>(),
                        It.IsAny<string>())).Returns(new List<string>());
            _handler.Setup(h => h.SaveFile(It.IsAny<HttpPostedFileBase>(), It.IsAny<string>()))
                .Returns(new RecommenderFile());
            _datasetMapper.Setup(dm => dm.Map(It.IsAny<Dataset>(), It.IsAny<FilterViewModel>())).Returns(new SingleDatasetViewModel());
            // Act
            var result = _uc.UploadFile(String.Empty, null, ";", true);
            // Assert
        }

        [TestMethod]
        public void TestUploadCreateDataset()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            _data.Setup(d => d.GetDataset(It.IsAny<int>())).Returns(dataset);
            _validations.Setup(
                v =>
                    v.DatatypesAreValid(It.IsAny<AttributeViewModel[]>(), It.IsAny<int>(), It.IsAny<string>(),
                        It.IsAny<string>())).Returns(new List<string>());
            _handler.Setup(h => h.SaveFile(It.IsAny<HttpPostedFileBase>(), It.IsAny<string>()))
                .Returns(new RecommenderFile());
            _datasetMapper.Setup(dm => dm.Map(It.IsAny<Dataset>(), It.IsAny<FilterViewModel>())).Returns(new SingleDatasetViewModel());
            // Act
            var result = _uc.CreateDataset(1, ";");
            // Assert
        }

        [TestMethod]
        public void TestUploadDefineDimensions()
        {
            // Arrange
            Reset();
            // Act
            var result = _uc.DefineDimensions(1, null);
            // Assert
        }

        [TestMethod]
        public void TestUploadCreateDatasetManually()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            _data.Setup(d => d.GetDataset(It.IsAny<int>())).Returns(dataset);
            _validations.Setup(
                v =>
                    v.DatatypesAreValid(It.IsAny<AttributeViewModel[]>(), It.IsAny<int>(), It.IsAny<string>(),
                        It.IsAny<string>())).Returns(new List<string>());
            // Act
            var result = _uc.CreateDatasetManually(1, new AttributeViewModel[] {}, ";", "dd.MM.yyyy");
            // Assert
        }

        [TestMethod]
        public void TestUploadCreateDatasetFromDsd()
        {
            // Arrange
            Reset();
            // Act
            var result = _uc.CreateDatasetFromDsd(1, null);
            // Assert
        }


        [TestMethod]
        public void TestMinedResultsMine()
        {
            // Arrange
            Reset();
            // Act
            var result = _mr.Mine(1);
            // Assert
        }

        [TestMethod]
        public void TestMinedResultsMineRules()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            _data.Setup(d => d.GetDataset(It.IsAny<int>())).Returns(dataset);
            // Act
            var result = _mr.MineRules(new MiningViewModel {CommensurabilityList = new List<CommensurabilityViewModel>()});
            // Assert
        }

    }
}
