using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender2.ViewModels;
using DataType = Recommender.Common.Enums.DataType;

namespace Recommender2.Validations
{
    public class InputValidations
    {
        private readonly IDataDecorator _data;
        private readonly CsvHandler _csvHandler;

        public InputValidations(IDataDecorator data, CsvHandler handler)
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
                (csvFile, attributes.Select(a =>(DataType) a.SelectedAttributeTypeId).ToArray(), separator, dateFormat);
            if (Enumerable.Any<string>(errors))
            {
                throw new ValidationException(Enumerable.Aggregate<string, string>(errors, string.Empty, (current, error) => current + (current + error)));
            }
        }
    }
}