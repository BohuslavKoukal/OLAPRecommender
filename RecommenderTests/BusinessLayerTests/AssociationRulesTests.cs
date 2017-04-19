using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.StarSchema;
using Recommender.Common;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class AssociationRulesTests
    {
        private readonly LispMinerConnector _connector;
        private readonly AssociationRulesTaskProcessor _processor;
        private readonly RulesPruner _pruner;
        private readonly AssociationRuleToViewMapper _mapper;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<IDataAccessLayer> _data;
        private readonly Mock<IStarSchemaQuerier> _querier;
        private readonly Mock<HttpClient> _client;

        public AssociationRulesTests()
        {
            _config = new Mock<IConfiguration>();
            _data = new Mock<IDataAccessLayer>();
            _querier = new Mock<IStarSchemaQuerier>();
            _client = new Mock<HttpClient>();
            _connector = new LispMinerConnector(_config.Object, _data.Object);
            _pruner = new RulesPruner();
            _processor = new AssociationRulesTaskProcessor(_connector, _data.Object, _config.Object, _pruner);
            _mapper = new AssociationRuleToViewMapper(_data.Object, _querier.Object);
        }

        private void Setup(List<Dimension> dimensions, DimensionTree tree)
        {
            _data.Setup(d => d.GetDimension(It.IsAny<int>())).Returns((int x) => dimensions.Single(d => d.Id == x));
            _data.Setup(d => d.GetAllDimValues(It.IsAny<int>())).Returns((int x) => dimensions.Single(d => d.Id == x).DimensionValues.ToList());
            _querier.Setup(q => q.GetValuesOfDimension(It.IsAny<DimensionDto>(), It.IsAny<Column>()))
                .Returns(
                    (DimensionDto d, Column c) =>
                            tree.GetDimensionDto(d.Id).DimensionValues.Where(dv => c.Value == dv.Value).ToList());
        }

        [TestMethod]
        public void TestMapRuleToView()
        {
            // Arrange
            var dataset = BusinessLayerTestHelper.CreateDataset(BusinessLayerTestHelper.DatasetName);
            var dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            var measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var tree = BusinessLayerTestHelper.CreateDimensionTree(BusinessLayerTestHelper.DatasetName);
            var task = BusinessLayerTestHelper.GetTask(dataset);
            var rule = BusinessLayerTestHelper.GetAssociationRule(dimensions, measures, task);
            Setup(dimensions, tree);
            // Act
            var ids = _mapper.GetXAndLegendDimensionsId(rule, tree);
            var filters = _mapper.GetFilterValues(rule).ToList();
            var text = _mapper.GetChartText(rule);
            // Assert
            ids.Item1.ShouldBeEquivalentTo(1);
            ids.Item2.ShouldBeEquivalentTo(4);
            filters.Count.ShouldBeEquivalentTo(1);
            filters[0].Name.ShouldBeEquivalentTo("Category");
            filters[0].DimensionValues[0].Value.ShouldBeEquivalentTo("Bakery");
            text.ShouldBeEquivalentTo("Association rule: ");
        }
    }
}
