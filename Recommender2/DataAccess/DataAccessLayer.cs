using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business.DTO;
using Recommender2.Models;

namespace Recommender2.DataAccess
{
    public class DataAccessLayer
    {
        private readonly DbContext _dbContext;

        public DataAccessLayer(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Insert(Dataset dataset)
        {
            _dbContext.Datasets.Add(dataset);
            _dbContext.SaveChanges();
        }

        public void PopulateDataset(int id, ICollection<Measure> measures, ICollection<Dimension> dimensions)
        {
            var dataset = _dbContext.Datasets.Single(d => d.Id == id);
            foreach (var measure in measures)
            {
                measure.DataSet = dataset;
                _dbContext.Measures.Add(measure);
            }
            foreach (var dimension in dimensions)
            {
                dimension.DataSet = dataset;
                _dbContext.Dimensions.Add(dimension);
            }
            _dbContext.SaveChanges();
        }

        public string GetCsvFilePath(int id)
        {
            return GetDataset(id).CsvFilePath;
        }

        public List<Dataset> GetAllDatasets()
        {
            return _dbContext.Datasets.ToList();
        }

        public Dataset GetDataset(string name)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Name == name);
        }

        public Dataset GetDataset(int id)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Id == id);
        }

        public List<Dimension> GetChildDimensions(int id)
        {
            return _dbContext.Dimensions.Where(d => d.ParentDimension.Id == id).ToList();
        }

        public Dimension GetDimension(int id)
        {
            return _dbContext.Dimensions.Single(d => d.Id == id);
        }

        public Measure GetMeasure(int id)
        {
            return _dbContext.Measures.Single(m => m.Id == id);
        }

    }
}