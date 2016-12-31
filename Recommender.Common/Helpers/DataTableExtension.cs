using System;
using System.Data;
using System.Linq;

namespace Recommender.Common.Helpers
{
    public static class DataTableExtension
    {
        public static DataTable GetDistinctTable(this DataTable table, Type columnType, string columnName)
        {
            if (columnType == typeof(int))
            {
                return table.AsEnumerable()
                        .GroupBy(row => row.Field<int>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
            }
            if (columnType == typeof(double))
            {
                return table.AsEnumerable()
                        .GroupBy(row => row.Field<double>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
            }
            if (columnType == typeof(string))
            {
                return table.AsEnumerable()
                        .GroupBy(row => row.Field<string>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
            }
            return table.AsEnumerable()
                        .GroupBy(row => row.Field<DateTime?>(columnName))
                        .Select(group => group.First()).CopyToDataTable();
        }
    }
}