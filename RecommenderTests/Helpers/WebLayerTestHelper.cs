using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Web.ViewModels;

namespace RecommenderTests.Helpers
{
    public static class WebLayerTestHelper
    {
        public static List<AttributeViewModel> GetAttributes()
        {
            return new List<AttributeViewModel>
            {
                new AttributeViewModel { Name = "Product", SelectedAttributeRoleId = 1, SelectedAttributeType = "String"},
                new AttributeViewModel { Name = "Category", SelectedAttributeParentName = "Product", SelectedAttributeRoleId = 1, SelectedAttributeType = "String"},
                new AttributeViewModel { Name = "Place", SelectedAttributeRoleId = 1, SelectedAttributeType = "String"},
                new AttributeViewModel { Name = "Region", SelectedAttributeParentName = "Place", SelectedAttributeRoleId = 1, SelectedAttributeType = "String"},
                new AttributeViewModel { Name = "Units", SelectedAttributeRoleId = 2, SelectedAttributeType = "Double"},
            };
        }

        public static GroupedGraphDto GetGroupedGraph()
        {
            var root1 = new GroupedGraphXAxisRootDto
            {
                Name = "Bakery",
                XAxisLeaves = new List<GraphXAxisLeafDto>
                {
                    new GroupedGraphXAxisLeafDto
                    {
                        Name = "Bread",
                        LegendValues = new List<GraphLegendValueDto>
                        {
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Europe"}, Value = 30},
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Asia"}, Value = 50},
                        }
                    },
                    new GroupedGraphXAxisLeafDto
                    {
                        Name = "Bun",
                        LegendValues = new List<GraphLegendValueDto>
                        {
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Europe"}, Value = 70},
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Asia"}, Value = 80},
                        }
                    },
                }
            };
            var root2 = new GroupedGraphXAxisRootDto
            {
                Name = "Dairy",
                XAxisLeaves = new List<GraphXAxisLeafDto>
                {
                    new GroupedGraphXAxisLeafDto
                    {
                        Name = "Milk",
                        LegendValues = new List<GraphLegendValueDto>
                        {
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Europe"}, Value = 15},
                            new GraphLegendValueDto {Legend = new GraphLegendDto {Name = "Asia"}, Value = 35},
                        }
                    }
                }
            };
            return new GroupedGraphDto
            {
                Name = "Grouped graph",
                Roots = new List<GraphXAxisRootDto>
                {
                    root1, root2
                }
            };
        }

        public static DrilldownGraphDto GetDrilldownGraph()
        {
            var root1 = new DrilldownGraphXAxisRootDto
            {
                Name = "Bakery",
                XAxisLeaves = new List<GraphXAxisLeafDto>
                {
                    new DrilldownGraphXAxisLeafDto
                    {
                        Name = "Bread",
                        Value = 80
                    },
                    new DrilldownGraphXAxisLeafDto
                    {
                        Name = "Bun",
                        Value = 150
                    },
                }
            };
            var root2 = new DrilldownGraphXAxisRootDto
            {
                Name = "Dairy",
                XAxisLeaves = new List<GraphXAxisLeafDto>
                {
                    new DrilldownGraphXAxisLeafDto
                    {
                        Name = "Milk",
                        Value = 50
                    }
                }
            };
            return new DrilldownGraphDto
            {
                Name = "Drilldown graph",
                Roots = new List<GraphXAxisRootDto>
                {
                    root1, root2
                }
            };
        }
    }
}
