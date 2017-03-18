using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Recommender.Business.DTO;
using Recommender.Business.FileHandling.Csv;
using Recommender.Common;
using Recommender.Common.Enums;
using Recommender.Data.Models;
using Attribute = Recommender.Data.Models.Attribute;

namespace Recommender.Business.FileHandling
{
    public interface IFileHandler
    {
        RecommenderFile SaveFile(HttpPostedFileBase postedFile, string separator);
        DataTable GetValues(string csvFileName, List<DimensionOrMeasureDto> columns, string separator, string dateFormat);
    }

    public class FileHandler : IFileHandler
    {
        private readonly Configuration _configuration;
        private readonly CsvHandler _csvHandler;

        public FileHandler(Configuration configuration, CsvHandler csvHandler)
        {
            _configuration = configuration;
            _csvHandler = csvHandler;
        }

        public RecommenderFile SaveFile(HttpPostedFileBase postedFile, string separator)
        {
            var fileType = string.IsNullOrEmpty(separator) ? FileType.Ttl : FileType.Csv;
            var filePath = SavePostedFile(postedFile, fileType);
            RecommenderFile file;
            if (fileType == FileType.Csv)
            {
                var attributes = GetAttributes(filePath, separator.Single());
                file = new RecommenderFile
                {
                    FilePath = filePath,
                    Attributes = attributes.ToList()
                };
            }
            else
            {
                file = new RecommenderFile
                {
                    FilePath = filePath
                };
            }
            return file;
        }

        public DataTable GetValues(string csvFileName, List<DimensionOrMeasureDto> columns, string separator, string dateFormat)
        {
            return _csvHandler.GetValues(csvFileName, columns, separator, dateFormat);
        }

        private string SavePostedFile(HttpPostedFileBase postedFile, FileType fileType)
        {
            string filePathToSave = _configuration.GetFilesLocation() + new FileInfo(postedFile.FileName).Name;
            if (File.Exists(filePathToSave))
            {
                var oldName = Path.GetFileNameWithoutExtension(postedFile.FileName);
                var guid = Guid.NewGuid().ToString().Remove(7);
                filePathToSave = _configuration.GetFilesLocation() + oldName + guid;
                filePathToSave += fileType == FileType.Csv ? ".csv" : ".ttl";
            }
            byte[] content;
            using (var reader = new BinaryReader(postedFile.InputStream))
            {
                content = reader.ReadBytes(postedFile.ContentLength);
            }
            File.WriteAllBytes(filePathToSave, content);
            return filePathToSave;
        }

        private List<Attribute> GetAttributes(string filePath, char separator)
        {
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var firstLine = reader.ReadLine();
                var columns = firstLine.Split(separator);
                List<Tuple<string, Type>> attributeList = columns.Select(value => Tuple.Create(value, typeof(int))).ToList();
                return attributeList.Select(al => new Attribute { Name = al.Item1 }).ToList();
            }
        }
    }
}
