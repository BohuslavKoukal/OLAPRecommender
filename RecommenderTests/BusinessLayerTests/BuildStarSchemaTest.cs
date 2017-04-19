using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MySql.Data.MySqlClient;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Common.Constants;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;
using RecommenderTests.Helpers;
using IDbConnection = Recommender.Data.DataAccess.IDbConnection;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class BuildStarSchemaTest
    {
        private Mock<IDbConnection> _dbConnection;
        private Mock<IDataAccessLayer> _data;
        private readonly IQueryBuilder _queryBuilder;
        private readonly StarSchemaBuilder _testee;
        private const string DatasetName = "TestDataset";

        public BuildStarSchemaTest()
        {
            Setup();
            _queryBuilder = new QueryBuilder(_dbConnection.Object);
            _testee = new StarSchemaBuilder(_queryBuilder, _data.Object);
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
            BusinessLayerTestHelper.ResetDatabase();
        }

        private void SetupConnection()
        {
            _dbConnection.Setup(c => c.GetConnection()).Returns(new MySqlConnection(BusinessLayerTestHelper.DatabaseConnection));
        }

        [TestMethod]
        public void TestCreateAndFillDimensionTables()
        {
            // Arrange
            Reset();
            SetupConnection();
            var dataset = BusinessLayerTestHelper.CreateDataset(DatasetName);
            var dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            var values = BusinessLayerTestHelper.GetValuesDataTable();
            SetupDataMock(dimensions);
            // Act
            _testee.CreateAndFillDimensionTables(DatasetName, dimensions, values);
            var products = GetDimensionsValues(_queryBuilder.Select($"{DatasetName}Product", new List<Column>()));
            var categories = GetDimensionsValues(_queryBuilder.Select($"{DatasetName}Category", new List<Column>()));
            var places = GetDimensionsValues(_queryBuilder.Select($"{DatasetName}Place", new List<Column>()));
            var regions = GetDimensionsValues(_queryBuilder.Select($"{DatasetName}Region", new List<Column>()));
            // Assert
            products.Count.ShouldBeEquivalentTo(3);
            categories.Count.ShouldBeEquivalentTo(2);
            places.Count.ShouldBeEquivalentTo(3);
            regions.Count.ShouldBeEquivalentTo(2);
            products[0].Item1.ShouldBeEquivalentTo(1);
            products[0].Item2.ShouldBeEquivalentTo("Bread");
            products[1].Item1.ShouldBeEquivalentTo(2);
            products[1].Item2.ShouldBeEquivalentTo("Bun");
            products[2].Item1.ShouldBeEquivalentTo(3);
            products[2].Item2.ShouldBeEquivalentTo("Milk");
            categories[0].Item1.ShouldBeEquivalentTo(1);
            categories[0].Item2.ShouldBeEquivalentTo("Bakery");
            categories[1].Item1.ShouldBeEquivalentTo(2);
            categories[1].Item2.ShouldBeEquivalentTo("Dairy");
            places[0].Item1.ShouldBeEquivalentTo(1);
            places[0].Item2.ShouldBeEquivalentTo("Czech republic");
            places[1].Item1.ShouldBeEquivalentTo(2);
            places[1].Item2.ShouldBeEquivalentTo("Slovakia");
            places[2].Item1.ShouldBeEquivalentTo(3);
            places[2].Item2.ShouldBeEquivalentTo("Russia");
            regions[0].Item1.ShouldBeEquivalentTo(1);
            regions[0].Item2.ShouldBeEquivalentTo("Europe");
            regions[1].Item1.ShouldBeEquivalentTo(2);
            regions[1].Item2.ShouldBeEquivalentTo("Asia");
        }

        private List<Tuple<int, string>> GetDimensionsValues(DataTable table)
        {
            return (from DataRow row
                    in table.Rows
                                  select Tuple.Create(Convert.ToInt32(row[Constants.String.Id]), row[Constants.String.Value].ToString()))
                    .ToList();
        }

        private List<int> GetViewValues(DataTable table, Measure measure)
        {
            return (from DataRow row
                    in table.Rows
                    select Convert.ToInt32(row[measure.GetQualifiedName()]))
                    .ToList();
        }

        private void SetupDataMock(List<Dimension> dimensions)
        {
            _data.Setup(d => d.GetChildDimensions(It.IsAny<int>())).Returns((int x) => dimensions.Where(dim => dim.ParentDimension?.Id == x).ToList());
        }

        [TestMethod]
        public void TestCreateAndFillFactTableAndCreateView()
        {
            // Arrange
            Reset();
            SetupConnection();

            var dataset = BusinessLayerTestHelper.CreateDataset(DatasetName);
            var dimensions = BusinessLayerTestHelper.CreateDimensions(dataset);
            var values = BusinessLayerTestHelper.GetValuesDataTable();
            var measures = BusinessLayerTestHelper.CreateMeasures(dataset);
            var measure = measures[0];
            SetupDataMock(dimensions);
            // Act
            _testee.CreateAndFillDimensionTables(DatasetName, dimensions, values);
            _testee.CreateFactTable(dataset, dimensions, measures);
            _testee.FillFactTable(DatasetName, dimensions, measures, values);
            _testee.CreateView(dataset, dimensions, measures);
            var czechRepRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Place", "Czech republic")), measure);
            var slovakiaRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Place", "Slovakia")), measure);
            var russiaRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Place", "Russia")), measure);
            var breadRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Product", "Bread")), measure);
            var bunRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Product", "Bun")), measure);
            var milkRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Product", "Milk")), measure);
            var europeRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Region", "Europe")), measure);
            var asiaRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Region", "Asia")), measure);
            var bakeryRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Category", "Bakery")), measure);
            var dairyRows = GetViewValues(_queryBuilder.Select($"{DatasetName}View", GetSelector(dimensions, "Category", "Dairy")), measure);
            // Assert
            czechRepRows.Count.ShouldBeEquivalentTo(3);
            czechRepRows.Sum().ShouldBeEquivalentTo(80);
            slovakiaRows.Count.ShouldBeEquivalentTo(3);
            slovakiaRows.Sum().ShouldBeEquivalentTo(35);
            russiaRows.Count.ShouldBeEquivalentTo(3);
            russiaRows.Sum().ShouldBeEquivalentTo(165);

            breadRows.Count.ShouldBeEquivalentTo(3);
            breadRows.Sum().ShouldBeEquivalentTo(80);
            bunRows.Count.ShouldBeEquivalentTo(3);
            bunRows.Sum().ShouldBeEquivalentTo(150);
            milkRows.Count.ShouldBeEquivalentTo(3);
            milkRows.Sum().ShouldBeEquivalentTo(50);

            europeRows.Count.ShouldBeEquivalentTo(6);
            europeRows.Sum().ShouldBeEquivalentTo(115);
            asiaRows.Count.ShouldBeEquivalentTo(3);
            asiaRows.Sum().ShouldBeEquivalentTo(165);

            bakeryRows.Count.ShouldBeEquivalentTo(6);
            bakeryRows.Sum().ShouldBeEquivalentTo(230);
            dairyRows.Count.ShouldBeEquivalentTo(3);
            dairyRows.Sum().ShouldBeEquivalentTo(50);
        }

        private List<Column> GetSelector(List<Dimension> dimensions, string dimensionName, string dimensionValue)
        {
            return new List<Column> { new Column
            {
                Name = dimensions.Single(d => d.Name == dimensionName).GetQualifiedNameValue(),
                Type = typeof(string).ToString(),
                Value = dimensionValue }
            };
        }

    }
}
