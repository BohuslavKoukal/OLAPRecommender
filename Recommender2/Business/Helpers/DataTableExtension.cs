using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Recommender2.Business.Helpers
{
    public static class DataTableExtension
    {
        public static DataTable GetDistinctTable(this DataTable table, int columnType, string columnName)
        {
            switch (columnType)
            {
                case 0:
                    return table.AsEnumerable()
                        .GroupBy(row => row.Field<int>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
                case 1:
                    return table.AsEnumerable()
                        .GroupBy(row => row.Field<double>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
                case 2:
                    return table.AsEnumerable()
                        .GroupBy(row => row.Field<string>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
                default:
                    return table.AsEnumerable()
                        .GroupBy(row => row.Field<DateTime>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
            }
        }
    }
}