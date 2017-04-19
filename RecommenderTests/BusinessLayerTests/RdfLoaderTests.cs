using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recommender.Business.FileHandling.Rdf;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class RdfLoaderTests
    {
        private readonly RdfLoader _testee;
        private readonly RdfLoader _testeeWithHierarchies;

        public RdfLoaderTests()
        {
            _testee = new RdfLoader($"{BusinessLayerTestHelper.DataLocation}\\ustidsd.ttl", $"{BusinessLayerTestHelper.DataLocation}\\usti.ttl");
            _testeeWithHierarchies = new RdfLoader($"{BusinessLayerTestHelper.DataLocation}\\retail-dsd.ttl", $"{BusinessLayerTestHelper.DataLocation}\\RetailDataset.ttl");
        }

        [TestInitialize]
        public void Initialize()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void TestRdfLoaderGetDimensions()
        {
            // Arrange

            // Act
            var dimensions = _testee.GetDimensions(BusinessLayerTestHelper.DatasetName).ToList();
            // Assert
            dimensions.Count().ShouldBeEquivalentTo(5);
            dimensions[0].Name.ShouldBeEquivalentTo("Budget_paragraph");
            dimensions[1].Name.ShouldBeEquivalentTo("Budget_item");
            dimensions[2].Name.ShouldBeEquivalentTo("Budget_instrument");
            dimensions[3].Name.ShouldBeEquivalentTo("Spatial_unit");
            dimensions[4].Name.ShouldBeEquivalentTo("budgetPhase");
        }

        [TestMethod]
        public void TestRdfLoaderGetMeasures()
        {
            // Arrange

            // Act
            var measures = _testee.GetMeasures(BusinessLayerTestHelper.DatasetName).ToList();
            // Assert
            measures.Count().ShouldBeEquivalentTo(1);
            measures[0].Name.ShouldBeEquivalentTo("amount");
        }

        [TestMethod]
        public void TestRdfLoaderGetData()
        {
            // Arrange

            // Act
            var dimensions = _testee.GetDimensions(BusinessLayerTestHelper.DatasetName).ToList();
            var measures = _testee.GetMeasures(BusinessLayerTestHelper.DatasetName).ToList();
            var data = _testee.ConvertObservationsToDataTable(dimensions, measures);
            // Assert
            data.Rows.Count.ShouldBeEquivalentTo(111);
            data.Rows[0]["Budget_paragraph"].ShouldBeEquivalentTo("0000");
            data.Rows[0]["Budget_item"].ShouldBeEquivalentTo("4113");
            data.Rows[0]["Budget_instrument"].ShouldBeEquivalentTo("53");
            data.Rows[0]["Spatial_unit"].ShouldBeEquivalentTo("1");
            data.Rows[0]["budgetPhase"].ShouldBeEquivalentTo("approved");
            data.Rows[0]["amount"].ShouldBeEquivalentTo(572000);

            data.Rows[10]["Budget_paragraph"].ShouldBeEquivalentTo("0000");
            data.Rows[10]["Budget_item"].ShouldBeEquivalentTo("4116");
            data.Rows[10]["Budget_instrument"].ShouldBeEquivalentTo("33");
            data.Rows[10]["Spatial_unit"].ShouldBeEquivalentTo("1");
            data.Rows[10]["budgetPhase"].ShouldBeEquivalentTo("executed");
            data.Rows[10]["amount"].ShouldBeEquivalentTo(13717488.03);
        }

        [TestMethod]
        public void TestRdfLoaderGetDataWithHierarchies()
        {
            // Arrange

            // Act
            var dimensions = _testeeWithHierarchies.GetDimensions(BusinessLayerTestHelper.DatasetName).ToList();
            var measures = _testeeWithHierarchies.GetMeasures(BusinessLayerTestHelper.DatasetName).ToList();
            var data = _testeeWithHierarchies.ConvertObservationsToDataTable(dimensions, measures);
            // Assert
            data.Rows.Count.ShouldBeEquivalentTo(1);
            data.Rows[0]["Date"].ShouldBeEquivalentTo(new DateTime(2016, 1, 1));
            data.Rows[0]["Month"].ShouldBeEquivalentTo(new DateTime(2016, 1, 1));
            data.Rows[0]["Product"].ShouldBeEquivalentTo("Bread");
            data.Rows[0]["Category"].ShouldBeEquivalentTo("Bakery");
            data.Rows[0]["Country"].ShouldBeEquivalentTo("CZ");
            data.Rows[0]["Region"].ShouldBeEquivalentTo("EU");
            data.Rows[0]["Units_sold"].ShouldBeEquivalentTo(9);
            data.Rows[0]["Value_of_units_sold"].ShouldBeEquivalentTo(180);
        }
    }
}
