using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business;
using Recommender.Business.DTO;
using Recommender.Business.Service;
using Recommender.Data.DataAccess;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class GraphServiceTest
    {
        private Mock<IStarSchemaQuerier> _querier;
        private readonly GraphService _graphService;

        public GraphServiceTest()
        {
            Setup();
            _graphService = new GraphService(_querier.Object);
        }

        private void Setup()
        {
            _querier = new Mock<IStarSchemaQuerier>();
        }

        private void Reset()
        {
            _querier.Reset();
        }

        [TestMethod]
        public void TestGetGroupedGraphWithoutFilters()
        {
            // Arrange
            Reset();
            var tree = TestHelper.CreateDimensionTree("TestDataset");
            var xDimension = tree.GetDimensionDto(4);
            var legendDimension = tree.GetDimensionDto(2);
            var measure = new MeasureDto { Name = "Units" };
            SetupQuerierValuesOfDimensions(tree);
            SetupQuerierFactTableSum(measure);
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, new List<FlatDimensionDto>());
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            // Bakery, Diary
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[0].XAxisLeaves[1].Name.ShouldBeEquivalentTo("Bun");
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[1].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Milk");
            legendValues.Count.ShouldBeEquivalentTo(6);
            legendValues[0].Value.ShouldBeEquivalentTo(10);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            legendValues[2].Value.ShouldBeEquivalentTo(20);
            legendValues[3].Value.ShouldBeEquivalentTo(20);
            legendValues[4].Value.ShouldBeEquivalentTo(30);
            legendValues[5].Value.ShouldBeEquivalentTo(30);
        }

        [TestMethod]
        public void TestGetGroupedGraphWithOneFilter()
        {
            // Arrange
            Reset();
            var tree = TestHelper.CreateDimensionTree("TestDataset");
            // Category
            var xDimension = tree.GetDimensionDto(4);
            // Region
            var legendDimension = tree.GetDimensionDto(2);
            var measure = new MeasureDto { Name = "Units" };
            // Show only bakery
            var filters = new List<FlatDimensionDto>
            {
                new FlatDimensionDto
                {
                    Id = 4,
                    Name = "Category",
                    DatasetName = "TestDataset",
                    DimensionValues = new[] {new DimensionValueDto {Id = 1, Value = "Bakery"}}.ToList()
                }
            };
            SetupQuerierValuesOfDimensions(tree);
            SetupQuerierFactTableSum(measure);
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(1);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[0].XAxisLeaves[1].Name.ShouldBeEquivalentTo("Bun");
            legendValues.Count.ShouldBeEquivalentTo(4);
            legendValues[0].Value.ShouldBeEquivalentTo(10);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            legendValues[2].Value.ShouldBeEquivalentTo(20);
            legendValues[3].Value.ShouldBeEquivalentTo(20);
        }

        [TestMethod]
        public void TestGetGroupedGraphWithMoreFilters()
        {
            // Arrange
            Reset();
            var tree = TestHelper.CreateDimensionTree("TestDataset");
            // Category
            var xDimension = tree.GetDimensionDto(4);
            // Region
            var legendDimension = tree.GetDimensionDto(2);
            var measure = new MeasureDto { Name = "Units" };
            // Show only bread and milk in Europe
            var filters = TestHelper.GetBreadMilkEuropeFilters();
            SetupQuerierValuesOfDimensions(tree);
            SetupQuerierFactTableSum(measure);
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[1].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Milk");
            legendValues.Count.ShouldBeEquivalentTo(2);
            legendValues[0].Value.ShouldBeEquivalentTo(10);
            legendValues[0].Legend.Name.ShouldBeEquivalentTo("Europe");
            legendValues[1].Value.ShouldBeEquivalentTo(30);
            legendValues[1].Legend.Name.ShouldBeEquivalentTo("Europe");
        }

        [TestMethod]
        public void TestGetDrilldownGraphWithFilters()
        {
            // Arrange
            Reset();
            var tree = TestHelper.CreateDimensionTree("TestDataset");
            // Category
            var xDimension = tree.GetDimensionDto(4);
            var measure = new MeasureDto { Name = "Units" };
            // Show only bread and milk in Europe
            var filters = TestHelper.GetBreadMilkEuropeFilters();
            SetupQuerierValuesOfDimensions(tree);
            SetupQuerierFactTableSum(measure);
            // Act
            var graph = _graphService.GetDrilldownGraph(tree, xDimension, measure, filters);
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            var breadLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[0];
            var milkLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[1].XAxisLeaves[0];
            breadLeaf.Name.ShouldBeEquivalentTo("Bread");
            breadLeaf.Value.ShouldBeEquivalentTo(10);
            milkLeaf.Name.ShouldBeEquivalentTo("Milk");
            milkLeaf.Value.ShouldBeEquivalentTo(30);
        }

        [TestMethod]
        public void TestGetDrilldownGraphWithoutFilters()
        {
            // Arrange
            Reset();
            var tree = TestHelper.CreateDimensionTree("TestDataset");
            var xDimension = tree.GetDimensionDto(4);
            var measure = new MeasureDto { Name = "Units" };
            SetupQuerierValuesOfDimensions(tree);
            SetupQuerierFactTableSum(measure);
            // Act
            GraphDto graph = _graphService.GetDrilldownGraph(tree, xDimension, measure, new List<FlatDimensionDto>());
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            // Bakery, Diary
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            var breadLeaf = (DrilldownGraphXAxisLeafDto) graph.Roots[0].XAxisLeaves[0];
            var bunLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[1];
            var milkLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[1].XAxisLeaves[0];
            breadLeaf.Name.ShouldBeEquivalentTo("Bread");
            breadLeaf.Value.ShouldBeEquivalentTo(10);
            bunLeaf.Name.ShouldBeEquivalentTo("Bun");
            bunLeaf.Value.ShouldBeEquivalentTo(20);
            milkLeaf.Name.ShouldBeEquivalentTo("Milk");
            milkLeaf.Value.ShouldBeEquivalentTo(30);
        }

        private void SetupQuerierValuesOfDimensions(DimensionTree tree)
        {
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 1),
                It.Is<Column>(col => col.Name == "RegionId" && col.Value == "1")))
                .Returns(tree.GetDimensionDto(1).DimensionValues.Where(d => d.Id == 1 || d.Id == 2).ToList());
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 1),
                It.Is<Column>(col => col.Name == "RegionId" && col.Value == "2")))
                .Returns(tree.GetDimensionDto(1).DimensionValues.Where(d => d.Id == 2).ToList());
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 3), It.IsAny<Column>()))
                .Returns(tree.GetDimensionDto(2).DimensionValues);
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 3),
                It.Is<Column>(col => col.Name == "CategoryId" && col.Value == "1")))
                .Returns(tree.GetDimensionDto(3).DimensionValues.Where(d => d.Id == 1 || d.Id == 2).ToList());
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 3),
                It.Is<Column>(col => col.Name == "CategoryId" && col.Value == "2")))
                .Returns(tree.GetDimensionDto(3).DimensionValues.Where(d => d.Id == 3).ToList());
            _querier.Setup(c => c.GetValuesOfDimension(It.Is<DimensionDto>(d => d.Id == 4), It.IsAny<Column>()))
                .Returns(tree.GetDimensionDto(4).DimensionValues);
        }

        private void SetupQuerierFactTableSum(MeasureDto measure)
        {
            _querier.Setup(c => c.GetFactTableSum(It.IsAny<DimensionTree>(),
                It.IsAny<List<FlatDimensionDto>>(), It.Is<List<FlatDimensionDto>>(l => l[0].DimensionValues[0].Id == 1), It.Is<MeasureDto>(m => m.Name == measure.Name)))
                .Returns(10);
            _querier.Setup(c => c.GetFactTableSum(It.IsAny<DimensionTree>(),
                It.IsAny<List<FlatDimensionDto>>(), It.Is<List<FlatDimensionDto>>(l => l[0].DimensionValues[0].Id == 2), It.Is<MeasureDto>(m => m.Name == measure.Name)))
                .Returns(20);
            _querier.Setup(c => c.GetFactTableSum(It.IsAny<DimensionTree>(),
                It.IsAny<List<FlatDimensionDto>>(), It.Is<List<FlatDimensionDto>>(l => l[0].DimensionValues[0].Id == 3), It.Is<MeasureDto>(m => m.Name == measure.Name)))
                .Returns(30);
        }

    }
}
