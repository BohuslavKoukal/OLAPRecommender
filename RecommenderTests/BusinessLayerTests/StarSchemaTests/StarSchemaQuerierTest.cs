using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MySql.Data.MySqlClient;
using Recommender.Business;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests.StarSchemaTests
{
    [TestClass]
    public class StarSchemaQuerierTest
    {
        private Mock<IQueryBuilder> _queryBuilder;
        private Mock<IDataAccessLayer> _data;
        private readonly StarSchemaQuerier _querier;

        public StarSchemaQuerierTest()
        {
            Setup();
            _querier = new StarSchemaQuerier(_queryBuilder.Object, _data.Object);
        }

        private void Setup()
        {
            _queryBuilder = new Mock<IQueryBuilder>();
            _data = new Mock<IDataAccessLayer>();
        }

        private void Reset()
        {
            _queryBuilder.Reset();
            _data.Reset();
        }

        [TestMethod]
        public void TestGetValuesOfDimensionForString()
        {
            // Arrange
            Reset();
            var dataTable = StarSchemaQuerierTestHelper.CreateSampleDimensionDataTable(typeof(string));
            _queryBuilder.Setup(c => c.Select(It.Is<string>(s => s == "TestDatasetTestDimension"), It.IsAny<List<Column>>()))
            .Returns(dataTable);
            var dimension = new FlatDimensionDto
            {
                DatasetName = "TestDataset",
                Name = "TestDimension",
                Type = typeof(string)
            };
            // Act
            var result = _querier.GetValuesOfDimension(dimension);
            // Assert
            result.Should().HaveCount(3);
            result.First().Id.ShouldBeEquivalentTo(1);
            result.First().Value.ShouldBeEquivalentTo("Bread");
        }

        [TestMethod]
        public void TestGetValuesOfDimensionForDateTime()
        {
            // Arrange
            Reset();
            var dataTable = StarSchemaQuerierTestHelper.CreateSampleDimensionDataTable(typeof(DateTime));
            _queryBuilder.Setup(c => c.Select(It.Is<string>(s => s == "TestDatasetTestDimension"), It.IsAny<List<Column>>()))
            .Returns(dataTable);
            var dimension = new FlatDimensionDto
            {
                DatasetName = "TestDataset",
                Name = "TestDimension",
                Type = typeof(DateTime)
            };
            // Act
            var result = _querier.GetValuesOfDimension(dimension);
            // Assert
            result.Should().HaveCount(3);
            result.First().Id.ShouldBeEquivalentTo(1);
            result.First().Value.ShouldBeEquivalentTo("24.12.2016");
        }

        [TestMethod]
        public void TestGetFactTableSumWithNonRootCondition()
        {
            // Arrange
            Reset();
            var measure = new MeasureDto { Name = "Units" };
            var datasetName = "TestDataset";
            var factTableValues = new List<double> {1, 7.45, 58.12};
            var factDataTable = StarSchemaQuerierTestHelper.CreateSampleFactDataTable(measure.Name, factTableValues);
            var placeDataTable = TestHelper.CreatePlaceDimensionDataTable(1);
            var dimensionTree = TestHelper.CreateDimensionTree(datasetName);
            var europeDimensionValue = new DimensionValueDto {Id = 1, Value = "Europe"};
            var conditions = new[] { StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Region", 2, new[] {europeDimensionValue}.ToList()) }.ToList();
            _queryBuilder.Setup(c => c.Select(It.Is<string>(s => s == datasetName + "FactTable"),
                It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("PlaceId") && lc[0][0].Value.Equals("1") &&
                lc[0][1].Name.Equals("PlaceId") && lc[0][1].Value.Equals("2"))))
            .Returns(factDataTable);
            _queryBuilder.Setup(c => c.Select(It.Is<string>(s => s.Equals(datasetName + "Place")),
                It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("RegionId") && lc[0][0].Value.Equals("1"))))
            .Returns(placeDataTable);
            // Act
            var result = _querier.GetFactTableSum(dimensionTree, new List<FlatDimensionDto>(), conditions, measure);
            // Assert
            result.ShouldBeEquivalentTo(factTableValues.Sum());
        }

        [TestMethod]
        public void TestGetFactTableSumWithRootCondition()
        {
            // Arrange
            Reset();
            var measure = new MeasureDto { Name = "Units" };
            var datasetName = "TestDataset";
            var factTableValues = new List<double> { 1, 7.45, 58.12 };
            var factDataTable = StarSchemaQuerierTestHelper.CreateSampleFactDataTable(measure.Name, factTableValues);
            var dimensionTree = TestHelper.CreateDimensionTree(datasetName);
            var slovakiaDimensionValue = new DimensionValueDto { Id = 2, Value = "Slovakia" };
            var conditions = new[] { StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Place", 1, new[] { slovakiaDimensionValue }.ToList()) }.ToList();
            _queryBuilder.Setup(c => c.Select(It.Is<string>(s => s == datasetName + "FactTable"),
                It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("PlaceId") && lc[0][0].Value.Equals("2"))))
            .Returns(factDataTable);
            // Act
            var result = _querier.GetFactTableSum(dimensionTree, new List<FlatDimensionDto>(), conditions, measure);
            // Assert
            result.ShouldBeEquivalentTo(factTableValues.Sum());
        }

        [TestMethod]
        public void TestGetFactTableSumWithNonRootFilters()
        {
            // Arrange
            Reset();
            var measure = new MeasureDto { Name = "Units" };
            var datasetName = "TestDataset";
            var factTableValues = new List<double> { 1, 7.45, 58.12 };
            var factDataTable = StarSchemaQuerierTestHelper.CreateSampleFactDataTable(measure.Name, factTableValues);
            var placeDataTable = TestHelper.CreatePlaceDimensionDataTable(1);
            var productDataTable = TestHelper.CreateProductDimensionDataTable(1);
            var dimensionTree = TestHelper.CreateDimensionTree(datasetName);
            var europeDimensionValue = new DimensionValueDto { Id = 1, Value = "Europe" };
            var bakeryDimensionValue = new DimensionValueDto { Id = 1, Value = "Bakery" };
            var conditions = new[]
            {
                StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Region", 2, new[] { europeDimensionValue }.ToList())
            }.ToList();
            var filters = new[]
            {
                StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Category", 4, new[] { bakeryDimensionValue }.ToList())
            }.ToList();
            _queryBuilder.Setup(c => c.Select(
                    It.Is<string>(s => s == datasetName + "FactTable"),
                    It.Is<List<List<Column>>>
                        // Czech republic - from condition Europe
                        (lc => lc[1][0].Name.Equals("PlaceId") && lc[1][0].Value.Equals("1") &&
                               //Slovakia - from condition Europe
                               lc[1][1].Name.Equals("PlaceId") && lc[1][1].Value.Equals("2") &&
                               // Bread - from filter category
                               lc[0][0].Name.Equals("ProductId") && lc[0][0].Value.Equals("1") &&
                               // Bun - from filter category
                               lc[0][1].Name.Equals("ProductId") && lc[0][1].Value.Equals("2"))))
                .Returns(factDataTable);
            // Traverse to get place ids from region
            _queryBuilder.Setup(c => c.Select(
                    It.Is<string>(s => s.Equals(datasetName + "Place")),
                    It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("RegionId") && lc[0][0].Value.Equals("1"))))
                .Returns(placeDataTable);
            // Traverse to get product ids from category
            _queryBuilder.Setup(c => c.Select(
                    It.Is<string>(s => s.Equals(datasetName + "Product")),
                    It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("CategoryId") && lc[0][0].Value.Equals("1"))))
                .Returns(productDataTable);
            // Act
            var result = _querier.GetFactTableSum(dimensionTree, filters, conditions, measure);
            // Assert
            result.ShouldBeEquivalentTo(factTableValues.Sum());
        }

        [TestMethod]
        public void TestGetFactTableSumWithRootFilters()
        {
            // Arrange
            Reset();
            var measure = new MeasureDto { Name = "Units" };
            var datasetName = "TestDataset";
            var factTableValues = new List<double> { 1, 7.45, 58.12 };
            var factDataTable = StarSchemaQuerierTestHelper.CreateSampleFactDataTable(measure.Name, factTableValues);
            var placeDataTable = TestHelper.CreatePlaceDimensionDataTable(1);
            var dimensionTree = TestHelper.CreateDimensionTree(datasetName);
            var europeDimensionValue = new DimensionValueDto { Id = 1, Value = "Europe" };
            var russiaDimensionValue = new DimensionValueDto { Id = 3, Value = "Russia" };
            var conditions = new[]
            {
                StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Region", 2, new[] { europeDimensionValue }.ToList())
            }.ToList();
            var filters = new[]
            {
                StarSchemaQuerierTestHelper.CreateFilter(datasetName, "Place", 1, new[] { russiaDimensionValue }.ToList())
            }.ToList();
            _queryBuilder.Setup(c => c.Select(
                It.Is<string>(s => s == datasetName + "FactTable"),
                It.Is<List<List<Column>>>
                // Russia - from filter Russia
                (lc => lc[0][0].Name.Equals("PlaceId") && lc[0][0].Value.Equals("3") &&
                // Czech republic - from condition Europe
                lc[1][0].Name.Equals("PlaceId") && lc[1][0].Value.Equals("1") &&
                //Slovakia - from condition Europe
                lc[1][1].Name.Equals("PlaceId") && lc[1][1].Value.Equals("2")
                )))
            .Returns(factDataTable);
            _queryBuilder.Setup(c => c.Select(
                It.Is<string>(s => s.Equals(datasetName + "Place")),
                It.Is<List<List<Column>>>(lc => lc[0][0].Name.Equals("RegionId") && lc[0][0].Value.Equals("1"))))
            .Returns(placeDataTable);
            // Act
            var result = _querier.GetFactTableSum(dimensionTree, filters, conditions, measure);
            // Assert
            result.ShouldBeEquivalentTo(factTableValues.Sum());
        }

    }
}
