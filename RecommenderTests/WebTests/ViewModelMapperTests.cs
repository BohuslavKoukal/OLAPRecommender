using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business;
using Recommender.Business.DTO;
using Recommender.Business.StarSchema;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;
using RecommenderTests.Helpers;

namespace RecommenderTests.WebTests
{
    [TestClass]
    public class ViewModelMapperTests
    {
        private readonly BrowseCubeViewModelMapper _bcvmm;
        private readonly AttributeViewModelMapper _avmm;
        private readonly MiningTaskViewModelMapper _mtvmm;
        private readonly DatasetViewModelMapper _dvmm;
        private readonly Mock<IDimensionTreeBuilder> _treeBuilder;

        public ViewModelMapperTests()
        {
            _treeBuilder = new Mock<IDimensionTreeBuilder>();
            _mtvmm = new MiningTaskViewModelMapper();
            _dvmm = new DatasetViewModelMapper(_treeBuilder.Object, _mtvmm);
            _bcvmm = new BrowseCubeViewModelMapper(_dvmm);
            _avmm = new AttributeViewModelMapper();
        }

        [TestMethod]
        public void TestAttributeMapToDimensionDtos()
        {
            // Arrange
            var attributes = WebLayerTestHelper.GetAttributes();
            // Act
            var dtos = _avmm.MapToDimensionDtos(attributes).ToList();
            // Assert
            dtos.Count().ShouldBeEquivalentTo(4);
            dtos[0].Name.ShouldBeEquivalentTo("Product");
            dtos[0].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[1].Name.ShouldBeEquivalentTo("Category");
            dtos[1].ParentDimension.ShouldBeEquivalentTo(dtos[0]);
            dtos[1].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[2].Name.ShouldBeEquivalentTo("Place");
            dtos[2].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[3].Name.ShouldBeEquivalentTo("Region");
            dtos[3].ParentDimension.ShouldBeEquivalentTo(dtos[2]);
            dtos[3].Type.ShouldBeEquivalentTo(typeof(string));
        }

        [TestMethod]
        public void TestAttributeMapToMeasures()
        {
            // Arrange
            var attributes = WebLayerTestHelper.GetAttributes();
            // Act
            var measures = _avmm.MapToMeasures(attributes).ToList();
            // Assert
            measures.Count().ShouldBeEquivalentTo(1);
            measures[0].Name.ShouldBeEquivalentTo("Units");
            measures[0].Type.ShouldBeEquivalentTo(typeof(double).ToInt());
        }

        [TestMethod]
        public void TestAttributeMapToDimensions()
        {
            // Arrange
            var attributes = WebLayerTestHelper.GetAttributes();
            // Act
            var dimensions = _avmm.MapToDimensions(attributes).ToList();
            // Assert
            dimensions.Count().ShouldBeEquivalentTo(4);
            dimensions[0].Name.ShouldBeEquivalentTo("Product");
            dimensions[0].Type.ShouldBeEquivalentTo(typeof(string).ToInt());
            dimensions[1].Name.ShouldBeEquivalentTo("Category");
            dimensions[1].ParentDimension.ShouldBeEquivalentTo(dimensions[0]);
            dimensions[1].Type.ShouldBeEquivalentTo(typeof(string).ToInt());
            dimensions[2].Name.ShouldBeEquivalentTo("Place");
            dimensions[2].Type.ShouldBeEquivalentTo(typeof(string).ToInt());
            dimensions[3].Name.ShouldBeEquivalentTo("Region");
            dimensions[3].ParentDimension.ShouldBeEquivalentTo(dimensions[2]);
            dimensions[3].Type.ShouldBeEquivalentTo(typeof(string).ToInt());
        }

        [TestMethod]
        public void TestAttributeMapToDimensionsAndMeasures()
        {
            // Arrange
            var attributes = WebLayerTestHelper.GetAttributes();
            // Act
            var dtos = _avmm.MapToDimensionsAndMeasures(attributes).ToList();
            // Assert
            dtos.Count().ShouldBeEquivalentTo(5);
            dtos[0].Name.ShouldBeEquivalentTo("Product");
            dtos[0].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[1].Name.ShouldBeEquivalentTo("Category");
            dtos[1].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[2].Name.ShouldBeEquivalentTo("Place");
            dtos[2].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[3].Name.ShouldBeEquivalentTo("Region");
            dtos[3].Type.ShouldBeEquivalentTo(typeof(string));
            dtos[4].Name.ShouldBeEquivalentTo("Units");
            dtos[4].Type.ShouldBeEquivalentTo(typeof(double));
        }

        [TestMethod]
        public void TestMiningTaskMapTask()
        {
            // Arrange
            var task =
                BusinessLayerTestHelper.GetTask(
                    BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName));
            // Act
            var model = _mtvmm.Map(task);
            // Assert
            model.Aad.ShouldBeEquivalentTo(2);
            model.Base.ShouldBeEquivalentTo(15);
            model.Name.ShouldBeEquivalentTo("TestTask");
        }

        [TestMethod]
        public void TestMiningTaskMapDatasetAndCommensurabilities()
        {
            // Arrange
            var dataset =
                    BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            var tree = BusinessLayerTestHelper.CreateDimensionTree(BusinessLayerTestHelper.DatasetName);
            // Act
            var commensurabilities = _mtvmm.GetCommensurableDimensions(tree);
            var model = _mtvmm.Map(dataset, commensurabilities);
            // Assert
            model.CommensurabilityList.Single(c => c.Dimension.Name == "Place").Checked.ShouldBeEquivalentTo(false);
            model.CommensurabilityList.Single(c => c.Dimension.Name == "Region").Checked.ShouldBeEquivalentTo(true);
            model.CommensurabilityList.Single(c => c.Dimension.Name == "Product").Checked.ShouldBeEquivalentTo(false);
            model.CommensurabilityList.Single(c => c.Dimension.Name == "Category").Checked.ShouldBeEquivalentTo(true);
            model.BaseQ.ShouldBeEquivalentTo(1);
            model.Lift.ShouldBeEquivalentTo(2);
        }


        [TestMethod]
        public void TestMiningTaskMapModelDatasetAndCommensurableDimensions()
        {
            // Arrange
            var dataset =
                    BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            var model = new MiningViewModel { BaseQ = 10, Lift = 2, ConditionRequired = true};
            // Act
            var viewModel = _mtvmm.Map(model, dataset, BusinessLayerTestHelper.CreateDimensions(dataset));
            // Assert
            viewModel.Aad.ShouldBeEquivalentTo(1);
            viewModel.Base.ShouldBeEquivalentTo(10);
            viewModel.ConditionDimensions.Count.ShouldBeEquivalentTo(4);
            viewModel.ConditionRequired.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void TestBrowseCubeMapDataset()
        {
            // Arrange
            var dataset =
                    BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var filter = new FilterViewModel();
            // Act
            var model = _bcvmm.Map(dataset, filter);
            // Assert
            model.Dataset.Dimensions.Count.ShouldBeEquivalentTo(4);
            model.Dataset.Measures.Count.ShouldBeEquivalentTo(1);
            model.GroupedChart.ShouldBeEquivalentTo(null);
            model.DrilldownChart.ShouldBeEquivalentTo(null);
        }

        [TestMethod]
        public void TestBrowseCubeMapGroupedGraph()
        {
            // Arrange
            var graph = WebLayerTestHelper.GetGroupedGraph();
            // Act
            var model = _bcvmm.Map(graph);
            // Assert
            model.ChartTitle.ShouldBeEquivalentTo("Grouped graph");
            model.Categories.Count.ShouldBeEquivalentTo(2);
            model.Categories[0].Name.ShouldBeEquivalentTo("Bakery");
            model.Categories[0].Categories[0].Name.ShouldBeEquivalentTo("Bread");
            model.Categories[0].Categories[1].Name.ShouldBeEquivalentTo("Bun");
            model.Categories[1].Name.ShouldBeEquivalentTo("Dairy");
            model.Categories[1].Categories[0].Name.ShouldBeEquivalentTo("Milk");
            model.Series[0].Data.Length.ShouldBeEquivalentTo(6);
            model.Series[0].Data[0].ShouldBeEquivalentTo(30);
            model.Series[0].Data[1].ShouldBeEquivalentTo(50);
            model.Series[0].Data[2].ShouldBeEquivalentTo(70);
            model.Series[0].Data[3].ShouldBeEquivalentTo(80);
            model.Series[0].Data[4].ShouldBeEquivalentTo(15);
            model.Series[0].Data[5].ShouldBeEquivalentTo(35);
        }

        [TestMethod]
        public void TestBrowseCubeMapDrilldownGraph()
        {
            // Arrange
            var graph = WebLayerTestHelper.GetDrilldownGraph();
            // Act
            var model = _bcvmm.Map(graph);
            // Assert
            model.ChartTitle.ShouldBeEquivalentTo("Drilldown graph");
            model.Series[0].Data[0].Name.ShouldBeEquivalentTo("Bakery");
            model.Series[0].Data[0].Y.ShouldBeEquivalentTo(230);
            model.Series[0].Data[1].Name.ShouldBeEquivalentTo("Dairy");
            model.Series[0].Data[1].Y.ShouldBeEquivalentTo(50);
            model.Drilldown.Series[0].Data[0].Name.ShouldBeEquivalentTo("Bread");
            model.Drilldown.Series[0].Data[0].Y.ShouldBeEquivalentTo(80);
            model.Drilldown.Series[0].Data[1].Name.ShouldBeEquivalentTo("Bun");
            model.Drilldown.Series[0].Data[1].Y.ShouldBeEquivalentTo(150);
            model.Drilldown.Series[1].Data[0].Name.ShouldBeEquivalentTo("Milk");
            model.Drilldown.Series[1].Data[0].Y.ShouldBeEquivalentTo(50);
        }

        [TestMethod]
        public void TestBrowseCubeMapTree()
        {
            // Arrange
            var tree = BusinessLayerTestHelper.CreateDimensionTree(BusinessLayerTestHelper.DatasetName);
            // Act
            var model = _bcvmm.Map(tree);
            // Assert
            model.Dimensions.Count.ShouldBeEquivalentTo(4);
            model.Dimensions[0].DimensionName.ShouldBeEquivalentTo("Place");
            model.Dimensions[0].Values[0].Value.ShouldBeEquivalentTo("Czech republic");
            model.Dimensions[0].Values[0].Checked.ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void TestDatasetMapDatasets()
        {
            // Arrange
            var dataset1 = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName + "1");
            var dataset2 = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName + "2");
            // Act
            var model = _dvmm.Map(new List<Dataset> {dataset1, dataset2});
            // Assert
            model.Datasets.Count.ShouldBeEquivalentTo(2);
            model.Datasets[0].Name.ShouldBeEquivalentTo(BusinessLayerTestHelper.DatasetName + "1");
            model.Datasets[1].Name.ShouldBeEquivalentTo(BusinessLayerTestHelper.DatasetName + "2");
        }

    }
}
