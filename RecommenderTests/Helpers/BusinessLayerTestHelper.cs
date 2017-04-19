using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using MySql.Data.MySqlClient;
using Recommender.Business.DTO;
using Recommender.Common.Helpers;
using Recommender.Data.Models;
using Recommender.Web.ViewModels;

namespace RecommenderTests.Helpers
{
    public static class BusinessLayerTestHelper
    {
        public static string ServerConnection = "Server = localhost; UID = root; Password = rootpassword";
        public static string DatabaseConnection = "Server=localhost;Database=testcubes;UID=root;Password=rootpassword";
        public static string DropSchema = "DROP SCHEMA IF EXISTS testcubes";
        public static string CreateSchema = "CREATE SCHEMA testcubes";
        public static string DatasetName = "TestDataset";
        public static string DataLocation = "BusinessLayerTests\\Data";

        public static void ResetDatabase()
        {
            using (var connection = new MySqlConnection(ServerConnection))
            {
                connection.Open();
                RunCommand(connection, DropSchema);
                RunCommand(connection, CreateSchema);
                connection.Close();
            }
        }

        public static DimensionTree CreateDimensionTree(string datasetName)
        {
            var tree = new DimensionTree(datasetName);
            var placeValues = new List<DimensionValueDto>
            {
                new DimensionValueDto {Id = 1, Value = "Czech republic"},
                new DimensionValueDto {Id = 2, Value = "Slovakia"},
                new DimensionValueDto {Id = 3, Value = "Russia"},
            };
            var regionValues = new List<DimensionValueDto>
            {
                new DimensionValueDto {Id = 1, Value = "Europe"},
                new DimensionValueDto {Id = 2, Value = "Asia"}
            };
            var productValues = new List<DimensionValueDto>
            {
                new DimensionValueDto {Id = 1, Value = "Bread"},
                new DimensionValueDto {Id = 2, Value = "Bun"},
                new DimensionValueDto {Id = 3, Value = "Milk"},
            };
            var categoryValues = new List<DimensionValueDto>
            {
                new DimensionValueDto {Id = 1, Value = "Bakery"},
                new DimensionValueDto {Id = 2, Value = "Dairy"}
            };
            tree.Add(CreateDimensionDto(datasetName, 1, "Place", null, typeof(string), placeValues));
            tree.Add(CreateDimensionDto(datasetName, 2, "Region", 1, typeof(string), regionValues));
            tree.Add(CreateDimensionDto(datasetName, 3, "Product", null, typeof(string), productValues));
            tree.Add(CreateDimensionDto(datasetName, 4, "Category", 3, typeof(string), categoryValues));
            return tree;
        }

        public static void RunCommand(MySqlConnection conn, string commandText)
        {
            var command = conn.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        private static TreeDimensionDto CreateDimensionDto(string datasetName, int id, string name, int? parentId, Type type, List<DimensionValueDto> values)
        {
            return new TreeDimensionDto
            {
                DatasetName = datasetName,
                Name = name,
                Id = id,
                ParentId = parentId,
                DimensionValues = values,
                Type = type
            };
        }

        public static MiningTask GetTask(Dataset dataset)
        {
            var dimension = new Dimension
            {
                Name = "Category"
            };
            return new MiningTask
            {
                AssociationRules = new List<AssociationRule>(),
                Aad = 2,
                Base = 15,
                ConditionDimensions = new List<Dimension> { dimension },
                ConditionRequired = false,
                DataSet = dataset,
                Name = "TestTask"
            };
        }

        public static AssociationRule GetAssociationRule(List<Dimension> dimensions, List<Measure> measures, MiningTask task)
        {
            return new AssociationRule
            {
                AntecedentValues = new List<DimensionValue> { dimensions.Single(d => d.Name == "Place").DimensionValues.Single(dv => dv.Value == "Czech republic") },
                ConditionValues = new List<DimensionValue> { dimensions.Single(d => d.Name == "Category").DimensionValues.Single(dv => dv.Value == "Bakery") },
                Succedents = new List<Succedent> { new Succedent { Measure = measures[0]} },
                MiningTask = task
            };
        }

        public static List<DimensionOrMeasureDto> GetDimensionOrMeasureDtos()
        {
            return new List<DimensionOrMeasureDto>
            {
                new DimensionOrMeasureDto { Name = "Place", Type = typeof(string) },
                new DimensionOrMeasureDto { Name = "Region", Type = typeof(string) },
                new DimensionOrMeasureDto { Name = "Product", Type = typeof(string) },
                new DimensionOrMeasureDto { Name = "Category", Type = typeof(string) },
                new DimensionOrMeasureDto { Name = "Units", Type = typeof(double) },
            };
        }

        public static List<FlatDimensionDto> GetFilters(DimensionTree tree, List<Tuple<int, int[]>> dimensionsAndValues)
        {
            var ret = new List<FlatDimensionDto>();
            foreach (var dimension in dimensionsAndValues)
            {
                var dimensionInTree = tree.GetDimensionDto(dimension.Item1);
                ret.Add(new FlatDimensionDto
                {
                    Id = dimensionInTree.Id,
                    Name = dimensionInTree.Name,
                    DatasetName = dimensionInTree.DatasetName,
                    DimensionValues = dimension.Item2.Select(dv => dimensionInTree.DimensionValues.Single(dvt => dvt.Id == dv)).ToList()
                });
            }
            return ret;
        }

        public static List<FlatDimensionDto> GetBreadMilkEuropeFilter(DimensionTree tree)
        {
            var dimensions = new List<Tuple<int, int[]>> { Tuple.Create(3, new[] { 1, 3 }), Tuple.Create(2, new[] { 1 }) };
            return GetFilters(tree, dimensions);
        }

        public static List<FlatDimensionDto> GetBakeryFilter(DimensionTree tree)
        {
            var dimensions = new List<Tuple<int, int[]>> { Tuple.Create(4, new[] { 1 }) };
            return GetFilters(tree, dimensions);
        }

        public static void PrepareDatabase(string datasetName)
        {

            var createCategoryTable = $"CREATE TABLE {datasetName}category (Id int(11), Value_ varchar(1024), PRIMARY KEY(Id))";
            var createRegionTable = $"CREATE TABLE {datasetName}region (Id int(11), Value_ varchar(1024), PRIMARY KEY(Id))";
            var createProductTable = $"CREATE TABLE {datasetName}product (Id int(11), Value_ varchar(1024), CategoryId int(11), PRIMARY KEY(Id))";
            var createPlaceTable = $"CREATE TABLE {datasetName}place (Id int(11), Value_ varchar(1024), RegionId int(11), PRIMARY KEY(Id))";
            var createFactTable = $"CREATE TABLE {datasetName}facttable (Id int(11), Units int(11), ProductId int(11), PlaceId int(11), PRIMARY KEY(Id))";
            var insertToCategory = $"INSERT INTO {datasetName}category (Id, Value_) VALUES ('1', 'Bakery'), ('2', 'Dairy')";
            var insertToRegion = $"INSERT INTO {datasetName}region (Id, Value_) VALUES ('1', 'Europe'), ('2', 'Asia')";
            var insertToProduct = $"INSERT INTO {datasetName}product (Id, Value_, CategoryId) VALUES ('1', 'Bread', '1'), ('2', 'Bun', '1'), ('3', 'Milk', '2')";
            var insertToPlace = $"INSERT INTO {datasetName}place (Id, Value_, RegionId) VALUES ('1', 'Czech republic', '1'), ('2', 'Slovakia', '1'), ('3', 'Russia', '2')";
            var insertToFact = $"INSERT INTO {datasetName}facttable (Id, PlaceId, ProductId, Units) VALUES " +
                               "('1', '1', '1', '20'), ('2', '1', '2', '50'), ('3', '1', '3', '10'), ('4', '2', '1', '10'), ('5', '2', '2', '20'), " +
                               "('6', '2', '3', '5'), ('7', '3', '1', '50'), ('8', '3', '2', '80'), ('9', '3', '3', '35')";
            ResetDatabase();
            using (var connection = new MySqlConnection(DatabaseConnection))
            {
                connection.Open();
                RunCommand(connection, createCategoryTable);
                RunCommand(connection, createRegionTable);
                RunCommand(connection, createProductTable);
                RunCommand(connection, createPlaceTable);
                RunCommand(connection, createFactTable);
                RunCommand(connection, insertToCategory);
                RunCommand(connection, insertToRegion);
                RunCommand(connection, insertToProduct);
                RunCommand(connection, insertToPlace);
                RunCommand(connection, insertToFact);
                connection.Close();
            }
        }

        public static FilterViewModel GetBreadMilkEuropeFiltersAsFilterViewModel()
        {
            var dimensions = new List<FilterDimensionViewModel>
            {
                new FilterDimensionViewModel
                {
                    DimensionId = 3,
                    DimensionName = "Product",
                    Values = new[]
                    {
                        new DimensionValueViewModel {Id = 1, Value = "Bread", Checked = true},
                        new DimensionValueViewModel {Id = 2, Value = "Bun", Checked = false},
                        new DimensionValueViewModel {Id = 3, Value = "Milk", Checked = true}
                    }.ToList()
                },
                new FilterDimensionViewModel
                {
                    DimensionId = 2,
                    DimensionName = "Region",
                    Values = new[]
                    {
                        new DimensionValueViewModel {Id = 1, Value = "Europe", Checked = true},
                        new DimensionValueViewModel {Id = 2, Value = "Asia", Checked = false}
                    }.ToList()
                }
            };
            return new FilterViewModel
            {
                Dimensions = dimensions
            };
        }

        public static List<Dimension> CreateDimensions(Dataset dataset)
        {
            var placeDimension = new Dimension
            {
                Name = "Place",
                Id = 1,
                DataSet = dataset,
                Type = typeof(string).ToInt(),
                DimensionValues = new[]
                {
                    new DimensionValue {Id = 1, Value = "Czech republic"},
                    new DimensionValue {Id = 2, Value = "Slovakia"},
                    new DimensionValue {Id = 3, Value = "Russia"}
                }
            };
            var regionDimension = new Dimension
            {
                Name = "Region",
                Id = 2,
                DataSet = dataset,
                Type = typeof(string).ToInt(),
                ParentDimension = placeDimension,
                DimensionValues = new[]
                {
                    new DimensionValue {Id = 1, Value = "Europe"},
                    new DimensionValue {Id = 2, Value = "Asia"}
                }
            };
            var productDimension = new Dimension
            {
                Name = "Product",
                Id = 3,
                DataSet = dataset,
                Type = typeof(string).ToInt(),
                DimensionValues = new[]
                {
                    new DimensionValue {Id = 1, Value = "Bread"},
                    new DimensionValue {Id = 2, Value = "Bun"},
                    new DimensionValue {Id = 3, Value = "Milk"},
                }
            };
            var categoryDimension = new Dimension
            {
                Name = "Category",
                Id = 4,
                DataSet = dataset,
                Type = typeof(string).ToInt(),
                ParentDimension = productDimension,
                DimensionValues = new[]
                {
                    new DimensionValue {Id = 1, Value = "Bakery"},
                    new DimensionValue {Id = 2, Value = "Dairy"}
                }
            };
            placeDimension.DimensionValues.ForEach(dv => dv.Dimension = placeDimension);
            regionDimension.DimensionValues.ForEach(dv => dv.Dimension = regionDimension);
            productDimension.DimensionValues.ForEach(dv => dv.Dimension = productDimension);
            categoryDimension.DimensionValues.ForEach(dv => dv.Dimension = categoryDimension);
            return new List<Dimension>
            {
                placeDimension, regionDimension, productDimension, categoryDimension
            };

        }

        public static DataTable GetValuesDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Place", typeof(string));
            table.Columns.Add("Region", typeof(string));
            table.Columns.Add("Product", typeof(string));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Units", typeof(int));
            table.Rows.Add("Czech republic", "Europe", "Bread", "Bakery", 20);
            table.Rows.Add("Czech republic", "Europe", "Bun", "Bakery", 50);
            table.Rows.Add("Czech republic", "Europe", "Milk", "Dairy", 10);
            table.Rows.Add("Slovakia", "Europe", "Bread", "Bakery", 10);
            table.Rows.Add("Slovakia", "Europe", "Bun", "Bakery", 20);
            table.Rows.Add("Slovakia", "Europe", "Milk", "Dairy", 5);
            table.Rows.Add("Russia", "Asia", "Bread", "Bakery", 50);
            table.Rows.Add("Russia", "Asia", "Bun", "Bakery", 80);
            table.Rows.Add("Russia", "Asia", "Milk", "Dairy", 35);
            return table;
        }

        public static Dataset CreateDataset(string name)
        {
            return new Dataset(name)
            {
                
            };
        }

        public static List<Measure> CreateMeasures(Dataset dataset)
        {
            return new List<Measure>
            {
                new Measure { Id = 1, Name = "Units", DataSet = dataset, Type = typeof(int).ToInt()}
            };
        }
    }
}
