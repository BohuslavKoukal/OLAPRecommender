using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MySql.Data.MySqlClient;
using Recommender.Business.DTO;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class DisplayGraphTest
    {
        private Mock<IDbConnection> _dbConnection;
        private Mock<IDataAccessLayer> _data;
        private readonly GraphService _graphService;
        private const string DatasetName = "TestDataset";
        private const string Connection = "Server=localhost;Database=testcubes;UID=root;Password=rootpassword";

        public DisplayGraphTest()
        {
            Setup();
            IQueryBuilder queryBuilder = new QueryBuilder(_dbConnection.Object);
            IStarSchemaQuerier querier = new StarSchemaQuerier(queryBuilder, _data.Object);
            _graphService = new GraphService(querier);
        }

        private void Setup()
        {
            _data = new Mock<IDataAccessLayer>();
            _dbConnection = new Mock<IDbConnection>();
            BusinessLayerTestHelper.PrepareDatabase(DatasetName);
        }

        private void Reset()
        {
            _dbConnection.Reset();
        }

        private void SetupConnection()
        {
            _dbConnection.Setup(c => c.GetConnection()).Returns(new MySqlConnection(Connection));
        }

        [TestMethod]
        public void TestGetGroupedGraphWithNoFilters()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var legendDimension = tree.GetDimensionDto(1); // legendDimension = place
            var measure = new Measure { Name = "Units" };
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, new List<FlatDimensionDto>(), true);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);

            graph.Roots.Count.ShouldBeEquivalentTo(2); // Bakery, Diary
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[0].XAxisLeaves[1].Name.ShouldBeEquivalentTo("Bun");
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[1].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Milk");

            legendValues.Count.ShouldBeEquivalentTo(9);
            // Bread
            legendValues[0].Value.ShouldBeEquivalentTo(20);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            legendValues[2].Value.ShouldBeEquivalentTo(50);
            // Bun
            legendValues[3].Value.ShouldBeEquivalentTo(50);
            legendValues[4].Value.ShouldBeEquivalentTo(20);
            legendValues[5].Value.ShouldBeEquivalentTo(80);
            // Milk
            legendValues[6].Value.ShouldBeEquivalentTo(10);
            legendValues[7].Value.ShouldBeEquivalentTo(5);
            legendValues[8].Value.ShouldBeEquivalentTo(35);
        }

        [TestMethod]
        public void TestGetGroupedGraphWithOneFilter()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var legendDimension = tree.GetDimensionDto(1); // legendDimension = place
            var measure = new Measure { Name = "Units" };
            // Show only bakery
            var filters = new List<FlatDimensionDto>
                {
                    new FlatDimensionDto
                    {
                        Id = 4,
                        Name = "Category",
                        DatasetName = DatasetName,
                        DimensionValues = new[] {new DimensionValueDto {Id = 1, Value = "Bakery"}}.ToList()
                    }
                };
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters, true);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);

            graph.Roots.Count.ShouldBeEquivalentTo(1); // Bakery
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[0].XAxisLeaves[1].Name.ShouldBeEquivalentTo("Bun");

            legendValues.Count.ShouldBeEquivalentTo(6);
            // Bread
            legendValues[0].Value.ShouldBeEquivalentTo(20);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            legendValues[2].Value.ShouldBeEquivalentTo(50);
            // Bun
            legendValues[3].Value.ShouldBeEquivalentTo(50);
            legendValues[4].Value.ShouldBeEquivalentTo(20);
            legendValues[5].Value.ShouldBeEquivalentTo(80);
        }

        [TestMethod]
        public void TestGetGroupedGraphWithTwoFilters()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var legendDimension = tree.GetDimensionDto(1); // legendDimension = place
            var measure = new Measure { Name = "Units" };
            // Show only bakery
            // Show only bread and milk in Europe
            var filters = BusinessLayerTestHelper.GetBreadMilkEuropeFilter(tree);
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters, true);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);

            graph.Roots.Count.ShouldBeEquivalentTo(2); // Bakery, Diary
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bread");
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            graph.Roots[1].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Milk");
            legendValues.Count.ShouldBeEquivalentTo(4);
            // Bread
            legendValues[0].Value.ShouldBeEquivalentTo(20);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            // Milk
            legendValues[2].Value.ShouldBeEquivalentTo(10);
            legendValues[3].Value.ShouldBeEquivalentTo(5);
        }

        [TestMethod]
        public void TestGetGroupedGraphWithTwoFiltersWithoutGrouping()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var legendDimension = tree.GetDimensionDto(1); // legendDimension = place
            var measure = new Measure { Name = "Units" };
            // Show only bakery
            // Show only bread and milk in Europe
            var filters = BusinessLayerTestHelper.GetBreadMilkEuropeFilter(tree);
            // Act
            var graph = _graphService.GetGroupedGraph(tree, xDimension, legendDimension, measure, filters, false);
            var legendValues = graph.GetLegendValues().ToList();
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(legendDimension.Name);
            graph.Name.Should().Contain(measure.Name);

            graph.Roots.Count.ShouldBeEquivalentTo(1);
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].XAxisLeaves[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves[1].Name.ShouldBeEquivalentTo("Dairy");
            legendValues.Count.ShouldBeEquivalentTo(4);
            // Bread
            legendValues[0].Value.ShouldBeEquivalentTo(20);
            legendValues[1].Value.ShouldBeEquivalentTo(10);
            // Milk
            legendValues[2].Value.ShouldBeEquivalentTo(10);
            legendValues[3].Value.ShouldBeEquivalentTo(5);
        }

        [TestMethod]
        public void TestGetDrilldownGraphWithNoFilters()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var measure = new Measure { Name = "Units" };
            // Act
            var graph = _graphService.GetDrilldownGraph(tree, xDimension, measure, new List<FlatDimensionDto>(), true);
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[1].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            var breadLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[0];
            var bunLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[1];
            var milkLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[1].XAxisLeaves[0];
            breadLeaf.Name.ShouldBeEquivalentTo("Bread");
            breadLeaf.Value.ShouldBeEquivalentTo(80);
            bunLeaf.Name.ShouldBeEquivalentTo("Bun");
            bunLeaf.Value.ShouldBeEquivalentTo(150);
            milkLeaf.Name.ShouldBeEquivalentTo("Milk");
            milkLeaf.Value.ShouldBeEquivalentTo(50);
        }

        [TestMethod]
        public void TestGetDrilldownGraphWithOneFilter()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var measure = new Measure { Name = "Units" };
            // Show only bakery
            var filters = BusinessLayerTestHelper.GetBakeryFilter(tree);
            // Act
            var graph = _graphService.GetDrilldownGraph(tree, xDimension, measure, filters, true);
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(1);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(2);
            var breadLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[0];
            var bunLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[1];
            breadLeaf.Name.ShouldBeEquivalentTo("Bread");
            breadLeaf.Value.ShouldBeEquivalentTo(80);
            bunLeaf.Name.ShouldBeEquivalentTo("Bun");
            bunLeaf.Value.ShouldBeEquivalentTo(150);
        }

        [TestMethod]
        public void TestGetDrilldownGraphWithTwoFilters()
        {
            // Arrange
            Reset();
            SetupConnection();
            var tree = BusinessLayerTestHelper.CreateDimensionTree(DatasetName);
            var xDimension = tree.GetDimensionDto(4); // xDimension = category
            var measure = new Measure { Name = "Units" };
            // Show only bakery
            // Show only bread and milk in Europe
            var filters = BusinessLayerTestHelper.GetBreadMilkEuropeFilter(tree);
            // Act
            var graph = _graphService.GetDrilldownGraph(tree, xDimension, measure, filters, true);
            // Assert
            graph.Name.Should().Contain(xDimension.Name);
            graph.Name.Should().Contain(measure.Name);
            graph.Roots.Count.ShouldBeEquivalentTo(2);
            graph.Roots[0].Name.ShouldBeEquivalentTo("Bakery");
            graph.Roots[1].Name.ShouldBeEquivalentTo("Dairy");
            graph.Roots[0].XAxisLeaves.Count.ShouldBeEquivalentTo(1);
            var breadLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[0].XAxisLeaves[0];
            var milkLeaf = (DrilldownGraphXAxisLeafDto)graph.Roots[1].XAxisLeaves[0];
            breadLeaf.Name.ShouldBeEquivalentTo("Bread");
            breadLeaf.Value.ShouldBeEquivalentTo(30);
            milkLeaf.Name.ShouldBeEquivalentTo("Milk");
            milkLeaf.Value.ShouldBeEquivalentTo(15);
        }
    }
}
