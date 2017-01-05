using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.FileHandling.Csv;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender2.ViewModels;

namespace Recommender2.Validations
{
    public class InputValidations
    {
        private readonly IDataAccessLayer _data;
        private readonly CsvHandler _csvHandler;

        public InputValidations(IDataAccessLayer data, CsvHandler handler)
        {
            _data = data;
            _csvHandler = handler;
        }

        public void DatasetNameIsValid(string name)
        {
            if (_data.GetDataset(name) != null)
            {
                throw new ValidationException($"Dataset {name} already exists.");
            }
        }

        public void UploadedFileIsValid(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ValidationException("Uploaded file is empty.");
            }
        }

        public void  DatatypesAreValid(AttributeViewModel[] attributes, int id, string separator, string dateFormat)
        {
            var csvFile = _data.GetCsvFilePath(id);
            var errors = _csvHandler.GetAttributeErrors
                (csvFile, attributes.Select(a => a.SelectedAttributeType.ToType()).ToArray(), separator, dateFormat);
            if (errors.Any())
            {
                throw new ValidationException(errors.Aggregate(string.Empty, (current, error) => current + (current + error)));
            }
        }

        public void DsdIsValid(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ValidationException("Uploaded file is empty.");
            }
        }
    }
}