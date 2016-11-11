using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recommender2.DataAccess;
using Recommender2.ViewModels;
using DataType = Recommender2.Business.Enums.DataType;

namespace Recommender2.Business.Validations
{
    public class InputValidations
    {
        private readonly DataAccessLayer _data;
        private readonly CsvHandler _csvHandler;

        public InputValidations(DataAccessLayer data, CsvHandler handler)
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

        public void  DatatypesAreValid(AttributeViewModel[] attributes, int id)
        {
            var csvFile = _data.GetCsvFilePath(id);
            var errors = _csvHandler.GetAttributeErrors
                (csvFile, attributes.Select(a =>(DataType) a.SelectedAttributeTypeId).ToArray());
            if (errors.Any())
            {
                throw new ValidationException(errors.Aggregate(string.Empty, (current, error) => current + (current + error)));
            }
        }
    }
}