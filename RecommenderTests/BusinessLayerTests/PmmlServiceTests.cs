using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.StarSchema;
using Recommender.Common;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class PmmlServiceTests
    {
        private readonly PmmlService _generateTestee;
        private readonly PmmlService _parseTestee;
        private readonly DataDiscretizator _discretizator;
        private readonly Mock<IStarSchemaQuerier> _querier;
        private readonly Mock<IDataAccessLayer> _data;
        private readonly IDimensionTreeBuilder _treeBuilder;

        public PmmlServiceTests()
        {
            var configuration = new Mock<IConfiguration>();
            _querier = new Mock<IStarSchemaQuerier>();
            _generateTestee = new PmmlService(configuration.Object);
            _parseTestee = new PmmlService(configuration.Object, $"{BusinessLayerTestHelper.DataLocation}\\sampleresults.pmml");
            _discretizator = new DataDiscretizator(_querier.Object);
            _data = new Mock<IDataAccessLayer>();
            _treeBuilder = new DimensionTreeBuilder(_data.Object, _querier.Object);
        }

        [TestMethod]
        public void TestPreprocessingPmml()
        {
            // Arrange
            Reset();
            SetupQuerier();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var task = BusinessLayerTestHelper.GetTask(dataset);
            var rowcount = 9;
            var discretizations = _discretizator.GetDiscretizations(dataset, rowcount);
            // Act
            var pmml = _generateTestee.GetPreprocessingPmml(task, discretizations, rowcount);
            // Assert
            var header = pmml.SelectNodes("//*[local-name()='Header']").Cast<XmlNode>().Single();
            var miningBuildTask = pmml.SelectNodes("//*[local-name()='MiningBuildTask']").Cast<XmlNode>().Single();
            var dataDictionary = pmml.SelectNodes("//*[local-name()='DataDictionary']").Cast<XmlNode>().Single();
            var transformationDictionary = pmml.SelectNodes("//*[local-name()='TransformationDictionary']").Cast<XmlNode>().Single();
            var derivedFields = pmml.SelectNodes("//*[local-name()='DerivedField']").Cast<XmlNode>().ToList();
            var columns = pmml.SelectNodes("//*[local-name()='Column']").Cast<XmlNode>().ToList();
            var discretizeBins = pmml.SelectNodes("//*[local-name()='DiscretizeBin']").Cast<XmlNode>().ToList();
            dataDictionary.Attributes["numberOfFields"].InnerText.ShouldBeEquivalentTo("6"); // Product, Category, Place, Region, Id, Units
            derivedFields.Count.ShouldBeEquivalentTo(6);
            columns.Count.ShouldBeEquivalentTo(7);
            discretizeBins.Count.ShouldBeEquivalentTo(5);
            discretizeBins[0].Attributes["binValue"].InnerText.ShouldBeEquivalentTo("[5;10]");
            discretizeBins[1].Attributes["binValue"].InnerText.ShouldBeEquivalentTo("[10;10]");
            discretizeBins[2].Attributes["binValue"].InnerText.ShouldBeEquivalentTo("[10;20]");
            discretizeBins[3].Attributes["binValue"].InnerText.ShouldBeEquivalentTo("[20;20]");
            discretizeBins[4].Attributes["binValue"].InnerText.ShouldBeEquivalentTo("[20;80]");
        }

        [TestMethod]
        public void TestTaskPmml()
        {
            // Arrange
            Reset();
            SetupQuerier();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var task = BusinessLayerTestHelper.GetTask(dataset);
            var rowcount = 9;
            var tree = _treeBuilder.ConvertToTree(dataset.Dimensions);
            var eqClasses = _discretizator.GetEquivalencyClasses(tree);
            // Act
            var pmml = _generateTestee.GetTaskPmml(task, eqClasses, rowcount);
            // Assert
            var header = pmml.SelectNodes("//*[local-name()='Header']").Cast<XmlNode>().Single();
            var associationModel = pmml.SelectNodes("//*[local-name()='AssociationModel']").Cast<XmlNode>().Single();
            var thresholds = pmml.SelectNodes("//*[local-name()='InterestMeasureThreshold']").Cast<XmlNode>().ToList();
            thresholds[0].ChildNodes.Item(0).InnerText.ShouldBeEquivalentTo("BASE");
            thresholds[0].ChildNodes.Item(1).InnerText.ShouldBeEquivalentTo("Greater than or equal");
            thresholds[0].ChildNodes.Item(2).InnerText.ShouldBeEquivalentTo("1");
            thresholds[1].ChildNodes.Item(0).InnerText.ShouldBeEquivalentTo("AAD");
            thresholds[1].ChildNodes.Item(1).InnerText.ShouldBeEquivalentTo("Greater than or equal");
            thresholds[1].ChildNodes.Item(2).InnerText.ShouldBeEquivalentTo("2");
        }

        [TestMethod]
        public void TestTaskState()
        {
            // Arrange
            Reset();
            // Act
            var taskState = _parseTestee.GetTaskState();
            // Assert
            taskState.ShouldBeEquivalentTo(TaskState.Finished);
        }

        [TestMethod]
        public void TestResultsPmml()
        {
            // Arrange
            Reset();
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            dataset.Dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            dataset.Measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var dimensionValues = new List<DimensionValue>();
            foreach (var dimension in dataset.Dimensions)
            {
                dimensionValues.AddRange(dimension.DimensionValues);
            }
            // Act
            var rules = _parseTestee.GetRules(dimensionValues, dataset.Measures.ToList());
            // Assert
            rules.Count.ShouldBeEquivalentTo(1);
            rules[0].AntecedentValues.ToList()[0].Dimension.Name.ShouldBeEquivalentTo("Product");
            rules[0].AntecedentValues.ToList()[0].Value.ShouldBeEquivalentTo("Bread");
            rules[0].Succedents.ToList()[0].Measure.Name.ShouldBeEquivalentTo("Units");
            rules[0].ConditionValues.ToList()[0].Dimension.Name.ShouldBeEquivalentTo("Category");
            rules[0].ConditionValues.ToList()[0].Value.ShouldBeEquivalentTo("Bakery");
        }

        private void Reset()
        {
            _querier.Reset();
        }

        private void SetupQuerier()
        {
            _querier.Setup(q => q.GetOrderedMeasureValues(It.IsAny<Measure>())).Returns(new List<double> { 5, 10, 10, 20, 20, 35, 50, 50, 80 });
        }
    }
}
