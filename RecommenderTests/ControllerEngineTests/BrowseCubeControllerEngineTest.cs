using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender.Web.ControllerEngine;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;
using RecommenderTests.Helpers;

namespace RecommenderTests.ControllerEngineTests
{
    [TestClass]
    public class BrowseCubeControllerEngineTest
    {
        private Mock<IDataAccessLayer> _dataMock;
        private Mock<IDatasetViewModelMapper> _datasetMapperMock;
        private Mock<IBrowseCubeViewModelMapper> _browseCubeMock;
        private Mock<IStarSchemaQuerier> _starSchemaQuerierMock;
        private Mock<IGraphService> _graphServiceMock;
        private Mock<IDimensionTreeBuilder>_treeBuilderMock;
        private Mock<IAssociationRuleToViewMapper> _ruleToViewMock;
        private readonly BrowseCubeControllerEngine _testee;

        public BrowseCubeControllerEngineTest()
        {
            Setup();
            _testee = new BrowseCubeControllerEngine(_dataMock.Object, _datasetMapperMock.Object, _browseCubeMock.Object,
                _starSchemaQuerierMock.Object,
                _graphServiceMock.Object, _treeBuilderMock.Object, _ruleToViewMock.Object);
        }

        private void Setup()
        {
            _dataMock = new Mock<IDataAccessLayer>();
            _datasetMapperMock = new Mock<IDatasetViewModelMapper>();
            _browseCubeMock = new Mock<IBrowseCubeViewModelMapper>();
            _ruleToViewMock = new Mock<IAssociationRuleToViewMapper>();
            _starSchemaQuerierMock = new Mock<IStarSchemaQuerier>();
            _graphServiceMock = new Mock<IGraphService>();
            _treeBuilderMock = new Mock<IDimensionTreeBuilder>();
        }

        private void Reset()
        {
            _dataMock.Reset();
            _datasetMapperMock.Reset();
            _browseCubeMock.Reset();
            _starSchemaQuerierMock.Reset();
            _graphServiceMock.Reset();
            _treeBuilderMock.Reset();
        }

        [TestMethod]
        public void TestGetDatasets()
        {
            // Arrange
            Reset();
            // Act
            _testee.GetDatasets();
            // Assert
            _dataMock.Verify(dm => dm.GetAllDatasets());
            _datasetMapperMock.Verify(dmm => dmm.Map(It.IsAny<IEnumerable<Dataset>>()));
        }

        [TestMethod]
        public void TestGetDataset()
        {
            // Arrange
            Reset();
            // Act
            _testee.GetDataset(1);
            // Assert
            _dataMock.Verify(dm => dm.GetDataset(1));
            _datasetMapperMock.Verify(dmm => dmm.Map(It.IsAny<Dataset>(), It.IsAny<FilterViewModel>()));
        }

        [TestMethod]
        public void TestBrowseCube()
        {
            // Arrange
            Reset();
            var tree = BusinessLayerTestHelper.CreateDimensionTree("TestDataset");
            _treeBuilderMock.Setup(c => c.ConvertToTree(It.IsAny<int>(), It.IsAny<bool>())).Returns(tree);
            _starSchemaQuerierMock.Setup(c => c.GetValuesOfDimension(It.IsAny<DimensionDto>(), It.IsAny<Column>()))
                .Returns((DimensionDto dim, Column c) => tree.GetDimensionDto(dim.Id).DimensionValues);
            // Act
            _testee.BrowseCube(1);
            // Assert
            _dataMock.Verify(dm => dm.GetDataset(1));
            _browseCubeMock.Verify(bcm => bcm.Map(It.IsAny<Dataset>(), It.IsAny<FilterViewModel>()));
            _browseCubeMock.Verify(bcm => bcm.Map(It.IsAny<DimensionTree>()));
        }

        [TestMethod]
        public void TestShowChartDefinedManually()
        {
            // Arrange
            Reset();
            var filters = BusinessLayerTestHelper.GetBreadMilkEuropeFiltersAsFilterViewModel();
            var tree = BusinessLayerTestHelper.CreateDimensionTree("TestDataset");
            SetupDataLayer(tree);
            // Act
            _testee.ShowChart(1, 1, 1, 2, true, filters);
            // Assert
            _dataMock.Verify(dm => dm.GetDataset(1));
            _dataMock.Verify(dm => dm.GetMeasure(1));
            // Filter predicates
            Predicate<List<FlatDimensionDto>> hasTwoMembers = l => l.Count == 2;
            Predicate<FlatDimensionDto> breadMilkFilterIsOk = 
                bmf => (bmf.Id == 3 && bmf.DimensionValues.Count == 2 
                && bmf.DimensionValues[0].Id == 1 && bmf.DimensionValues[1].Id == 3);
            Predicate<FlatDimensionDto> europeFilterIsOk =
                ef => (ef.Id == 2 && ef.DimensionValues.Count == 1 && ef.DimensionValues[0].Id == 1);

            _graphServiceMock.Verify(gs => gs.GetGroupedGraph(tree,
                It.Is<TreeDimensionDto>(td => td.Id == 1), It.Is<TreeDimensionDto>(d => d.Id == 2),
                It.Is<Measure>(m => m.Id == 1),
                It.Is<List<FlatDimensionDto>>(f => hasTwoMembers(f) && breadMilkFilterIsOk(f[0]) && europeFilterIsOk(f[1])), It.Is<bool>(b => b)));

            _graphServiceMock.Verify(gs => gs.GetDrilldownGraph(tree,
                It.Is<TreeDimensionDto>(td => td.Id == 1),
                It.Is<Measure>(m => m.Id == 1),
                It.Is<List<FlatDimensionDto>>(f => hasTwoMembers(f) && breadMilkFilterIsOk(f[0]) && europeFilterIsOk(f[1])), It.Is<bool>(b => b == false)));

            _browseCubeMock.Verify(bcm => bcm.Map(It.IsAny<GroupedGraphDto>()));
            _browseCubeMock.Verify(bcm => bcm.Map(It.IsAny<DrilldownGraphDto>()));
        }

        [TestMethod]
        public void TestShowChartForAssociationRule()
        {

        }

        private void SetupDataLayer(DimensionTree tree)
        {
            _treeBuilderMock.Setup(c => c.ConvertToTree(1, It.IsAny<bool>())).Returns(tree);
            _dataMock.Setup(c => c.GetDimension(It.IsAny<int>()))
                .Returns((int id) => new Dimension { Name = tree.GetDimensionDto(id).Name, Id = id });
            _dataMock.Setup(c => c.GetMeasure(1)).Returns(new Measure { Id = 1 });
            _starSchemaQuerierMock.Setup(c => c.GetValuesOfDimension(It.IsAny<DimensionDto>(), It.IsAny<Column>()))
                .Returns((DimensionDto dd, Column col) => tree.GetDimensionDto(dd.Id).DimensionValues);
        }
    }
}
