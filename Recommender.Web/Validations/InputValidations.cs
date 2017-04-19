using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.FileHandling.Csv;
using Recommender.Common.Helpers;
using Recommender.Data.DataAccess;
using Recommender.Web.ViewModels;

namespace Recommender.Web.Validations
{
    public interface IInputValidations
    {
        void DatasetNameIsValid(string name);
        void TaskNameIsValid(string name);
        void UploadedFileIsValid(HttpPostedFileBase file);
        List<string> DatatypesAreValid(AttributeViewModel[] attributes, int id, string separator, string dateFormat);
        void DsdIsValid(HttpPostedFileBase file);

    }

    public class InputValidations : IInputValidations
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

        public void TaskNameIsValid(string name)
        {
            if (_data.GetMiningTask(name) != null)
            {
                throw new ValidationException($"Task {name} already exists.");
            }
        }

        public void UploadedFileIsValid(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ValidationException("Uploaded file is empty.");
            }
            var extension = file.FileName.Split('.').Last();
            if (extension != "csv" && extension != "ttl")
            {
                throw new ValidationException("Uploaded file must be .csv or .ttl.");
            }
        }

        public List<string> DatatypesAreValid(AttributeViewModel[] attributes, int id, string separator, string dateFormat)
        {
            var csvFile = _data.GetCsvFilePath(id);
            var errors = _csvHandler.GetAttributeErrors
                (csvFile, attributes.Select(a => a.SelectedAttributeType.ToType()).ToArray(), separator, dateFormat);
            return errors.Any() ? GetFirst10Errors(errors) : errors;
        }

        public void DsdIsValid(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ValidationException("Uploaded file is empty.");
            }
        }

        private List<string> GetFirst10Errors(List<string> errors)
        {
            var boundary = errors.Count < 11 ? errors.Count : 10;
            var ret = new List<string>();
            for (var i = 0; i < boundary; i++)
            {
                ret.Add(errors[i]);
            }
            if (errors.Count > 10)
            {
                ret.Add($"And {errors.Count - 10} more errors...");
            }
            return ret;
        }
    }
}