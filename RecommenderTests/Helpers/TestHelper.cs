using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;

namespace RecommenderTests.Helpers
{
    public static class TestHelper
    {
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
            tree.Add(CreateDimensionDto(datasetName, 1, "Place", null, placeValues));
            tree.Add(CreateDimensionDto(datasetName, 2, "Region", 1, regionValues));
            tree.Add(CreateDimensionDto(datasetName, 3, "Product", null, productValues));
            tree.Add(CreateDimensionDto(datasetName, 4, "Category", 3, categoryValues));
            return tree;
        }

        public static DataTable CreatePlaceDimensionDataTable(int regionId)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Value", typeof(string));
            switch (regionId)
            {
                case 1:
                    table.Rows.Add(1, "Czech republic");
                    table.Rows.Add(2, "Slovakia");
                    break;
                case 2:
                    table.Rows.Add(3, "Russia");
                    break;
            }
            return table;
        }

        public static DataTable CreateProductDimensionDataTable(int categoryId)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Value", typeof(string));
            switch (categoryId)
            {
                case 1:
                    table.Rows.Add(1, "Bread");
                    table.Rows.Add(2, "Bun");
                    break;
                case 2:
                    table.Rows.Add(3, "Milk");
                    break;
            }
            return table;
        }

        public static List<FlatDimensionDto> GetBreadMilkEuropeFilters()
        {
            return new List<FlatDimensionDto>
            {
                new FlatDimensionDto
                {
                    Id = 3,
                    Name = "Product",
                    DatasetName = "TestDataset",
                    DimensionValues = new[]
                    {
                        new DimensionValueDto {Id = 1, Value = "Bread"},
                        new DimensionValueDto {Id = 3, Value = "Milk"}
                    }.ToList()
                },
                new FlatDimensionDto
                {
                    Id = 2,
                    Name = "Region",
                    DatasetName = "TestDataset",
                    DimensionValues = new[]
                    {
                        new DimensionValueDto {Id = 1, Value = "Europe"}
                    }.ToList()
                },
            };
        }

        public static Dictionary<int, Dictionary<int, bool>> GetBreadMilkEuropeFiltersAsDictionary()
        {
            var productFilter = new Dictionary<int, bool> {{1, true}, {2, false}, {3, true}};
            var categoryFilter = new Dictionary<int, bool> {{1, true}, {2, true}};
            var regionFilter = new Dictionary<int, bool> { {1, true}, {2, false} };
            return new Dictionary<int, Dictionary<int, bool>>
            {
                {3, productFilter},
                {4, categoryFilter},
                {2, regionFilter}
            };
        }

        private static TreeDimensionDto CreateDimensionDto(string datasetName, int id, string name, int? parentId, List<DimensionValueDto> values)
        {
            return new TreeDimensionDto
            {
                DatasetName = datasetName,
                Name = name,
                Id = id,
                ParentId = parentId,
                DimensionValues = values
            };
        }
    }
}
