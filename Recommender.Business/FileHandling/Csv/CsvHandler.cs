using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic.FileIO;
using Recommender.Business.DTO;
using Recommender.Common;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Business.FileHandling.Csv
{
    public interface ICsvHandler
    {
        List<string> GetAttributeErrors(string csvFileName, Type[] dataTypes, string separator, string dateFormat);
        DataTable GetValues(string csvFileName, List<DimensionOrMeasureDto> columns, string separator, string dateFormat);
    }

    public class CsvHandler : ICsvHandler
    {

        public DataTable GetValues(string csvFileName, List<DimensionOrMeasureDto> columns, string separator, string dateFormat)
        {
            var datatable = new DataTable();
            foreach (var column in columns)
            {
                datatable.Columns.Add(column.Name, column.Type);
            }
            using (var parser = new TextFieldParser(csvFileName))
            {
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(separator);
                // Do not validate first row
                parser.ReadFields();
                while (!parser.EndOfData)
                {
                    // Process row
                    string[] fields = parser.ReadFields();
                    if(fields.Length != columns.Count) throw new InvalidDataException("Number of fields does not correspond with number of columns.");
                    var objectArrayToAdd = new List<object>();
                    var row = datatable.NewRow();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        var type = columns[i].Type;
                        if (type == typeof(int))
                        {
                            objectArrayToAdd.Add(Convert.ToInt32(field));
                        }
                        else if (type == typeof(double))
                        {
                            objectArrayToAdd.Add(Convert.ToDouble(field));
                        }
                        else if (type == typeof(DateTime))
                        {
                            objectArrayToAdd.Add(DateTime.ParseExact(field, dateFormat, CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            objectArrayToAdd.Add(field.SafeName());
                        }
                    }
                    row.ItemArray = objectArrayToAdd.ToArray();
                    datatable.Rows.Add(row);
                }
            }
            return datatable;
        }

        public List<string> GetAttributeErrors(string csvFileName, Type[] dataTypes, string separator, string dateFormat)
        {
            var errors = new List<string>();
            using (var parser = new TextFieldParser(csvFileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(separator);
                // Do not validate first row
                parser.ReadFields();
                while (!parser.EndOfData)
                {
                    // Process row
                    string[] fields = parser.ReadFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var type = dataTypes[i];
                        if (type == typeof(DateTime))
                        {
                            DateTime dateTime;
                            var isDateTime = DateTime.TryParseExact(fields[i], dateFormat, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out dateTime);
                            if (!isDateTime)
                                errors.Add($"Value {fields[i]} is not datetime.");
                        }
                        else if (type == typeof(double))
                        {
                            double doublee;
                            var isDouble = double.TryParse(fields[i], out doublee);
                            if (!isDouble) errors.Add($"Value {fields[i]} is not double.");
                        }
                        else if (type == typeof(int))
                        {
                            int intee;
                            var isInt = int.TryParse(fields[i], out intee);
                            if (!isInt)
                                errors.Add($"Value {fields[i]} is not integer.");
                        }
                    }
                }
            }
            return errors;
        }

        


    }
}