using System;
using System.Collections.Generic;
using System.Linq;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender.Data.DataAccess
{
    public interface IDataAccessLayer
    {
        void Insert(Dataset dataset);
        void Insert(MiningTask task);
        void Insert(ICollection<AssociationRule> rules);
        void SaveTaskResults(int taskId, ICollection<AssociationRule> rules, int numberOfVerifications, TimeSpan taskDuration);
        void PopulateDataset(int id, ICollection<Measure> measures, ICollection<Dimension> dimensions, State state);
        string GetCsvFilePath(int id);
        List<Dataset> GetAllDatasets();
        List<DimensionValue> GetAllDimensionValues(int id);
        List<Measure> GetAllMeasures(int id);
        Dataset GetDataset(string name);
        Dataset GetDataset(int id);
        MiningTask GetMiningTask(int id);
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

        public void Insert(MiningTask task)
        {
            _dbContext.MiningTasks.Add(task);
            _dbContext.SaveChanges();
        }

        public void SaveTaskResults(int taskId, ICollection<AssociationRule> rules, int numberOfVerifications, TimeSpan taskDuration)
        {
            var task = _dbContext.MiningTasks.Single(t => t.Id == taskId);
            foreach (var associationRule in rules)
            {
                associationRule.MiningTask = task;
                task.AssociationRules.Add(associationRule);
            }
            task.TaskState = (int) TaskState.Finished;
            task.NumberOfVerifications = numberOfVerifications;
            task.TaskDuration = taskDuration;
            _dbContext.SaveChanges();
        }

        public void Insert(ICollection<AssociationRule> rules)
        {
            _dbContext.AssociationRules.AddRange(rules);
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

        public List<DimensionValue> GetAllDimensionValues(int id)
        {
            return _dbContext.DimensionValues.Where(dv => dv.Dimension.DataSet.Id == id).ToList();
        }

        public List<Measure> GetAllMeasures(int id)
        {
            return _dbContext.Measures.Where(dv => dv.DataSet.Id == id).ToList();
        }

        public Dataset GetDataset(string name)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Name == name);
        }

        public Dataset GetDataset(int id)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Id == id);
        }

        public MiningTask GetMiningTask(int id)
        {
            return _dbContext.MiningTasks.SingleOrDefault(mt => mt.Id == id);
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