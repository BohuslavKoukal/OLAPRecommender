using System;
using System.Data;
using System.Linq;

namespace Recommender.Common.Helpers
{
    public static class DataTableExtension
    {
        public static DataTable GetDistinctTable(this DataTable table, int columnType, string columnName)
        {
            switch (columnType)
            {
                case 0:
                    return table.AsEnumerable()
                        .GroupBy(row => DataRowExtensions.Field<int>(row, columnName))
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