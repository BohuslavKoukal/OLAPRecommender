using System.Collections.Generic;
using System.Linq;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender.Data.DataAccess
{
    public interface IDataAccessLayer
    {
        void Insert(Dataset dataset);
        void PopulateDataset(int id, ICollection<Measure> measures, ICollection<Dimension> dimensions, State state);
        string GetCsvFilePath(int id);
        List<Dataset> GetAllDatasets();
        Dataset GetDataset(string name);
        Dataset GetDataset(int id);
        List<Dimension> GetChildDimensions(int id);
        Dimension GetDimension(int id);
        Measure GetMeasure(int id);
    }

    public class DataAccessLayer : IDataAccessLayer
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

        public void PopulateDataset(int id, ICollection<Measure> measures, ICollection<Dimension> dimensions, State state)
        {
            var dataset = _dbContext.Datasets.Single(d => d.Id == id);
            dataset.State = state;
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