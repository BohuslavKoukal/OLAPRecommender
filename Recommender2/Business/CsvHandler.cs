using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic.FileIO;
using Recommender2.Business.Enums;
using Recommender2.Models;
using Attribute = Recommender2.Models.Attribute;

namespace Recommender2.Business
{
    public class CsvHandler
    {
        private readonly Configuration _configuration;
        private readonly string[] _dateFormats;

        public CsvHandler(Configuration configuration)
        {
            _configuration = configuration;
            _dateFormats = new[] {"MM.dd.yyyy", "MM.d.yyyy", "M.d.yyyy", "M.dd.yyyy"};
        }

        public CsvFile GetFile(HttpPostedFileBase postedFile)
        {
            var filePath = SavePostedFile(postedFile);
            var attributes = GetAttributes(filePath);
            var file = new CsvFile
            {
                FilePath = filePath,
                Attributes = attributes
            };

            return file;
        }

        public DataTable GetValues(string csvFileName)
        {
            string pathOnly = Path.GetDirectoryName(csvFileName);
            string fileName = Path.GetFileName(csvFileName);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + "Yes" + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable { Locale = CultureInfo.CurrentCulture };
                adapter.Fill(dataTable);
                return
                    dataTable;
            }
        }

        public List<string> GetAttributeErrors(string csvFileName, DataType[] dataTypes)
        {
            var errors = new List<string>();
            using (TextFieldParser parser = new TextFieldParser(csvFileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                // Do not validate first row
                parser.ReadFields();
                while (!parser.EndOfData)
                {
                    // Process row
                    string[] fields = parser.ReadFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var type = dataTypes[i];
                        switch (type)
                        {
                            case DataType.DateTime:
                                DateTime dateTime;
                                var isDateTime = DateTime.TryParseExact(fields[i], _dateFormats, CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out dateTime);
                                if(!isDateTime) errors.Add($"Value {fields[i]} is not datetime.");
                                break;
                            case DataType.Double:
                                double doublee;
                                var isDouble = Double.TryParse(fields[i], out doublee);
                                if (!isDouble) errors.Add($"Value {fields[i]} is not double.");
                                break;
                            case DataType.Integer:
                                int intee;
                                var isInt = Int32.TryParse(fields[i], out intee);
                                if (!isInt) errors.Add($"Value {fields[i]} is not integer.");
                                break;
                        }
                    }
                }
            }
            return errors;
        }

        private string SavePostedFile(HttpPostedFileBase postedFile)
        {
            string filePathToSave = _configuration.GetFilesLocation() + new FileInfo(postedFile.FileName).Name;
            if (File.Exists(filePathToSave))
            {
                var oldName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                var guid = Guid.NewGuid().ToString().Remove(7);
                filePathToSave = _configuration.GetFilesLocation() + oldName + guid + ".csv";
            }
            byte[] content;
            using (var reader = new BinaryReader(postedFile.InputStream))
            {
                content = reader.ReadBytes(postedFile.ContentLength);
            }
            File.WriteAllBytes(filePathToSave, content);
            return filePathToSave;
        }

        private List<Attribute> GetAttributes(string filePath)
        {
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var firstLine = reader.ReadLine();
                var columns = firstLine.Split(',');
                List<Tuple<string, DataType>> attributeList = columns.Select(value => Tuple.Create(value, DataType.Integer)).ToList();
                return attributeList.Select(al => new Attribute { Name = al.Item1 }).ToList();
            }
        }
    }
}