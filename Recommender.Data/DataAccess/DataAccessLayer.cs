using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
        void PopulateDataset(int id, ICollection<Measure> measures, ICollection<Dimension> dimensions);
        void ChangeDatasetState(State state);
        void SetTaskState(string userId, int taskId, int state, string failedReason = null);
        void SetPreprocessed(int datasetId);
        string GetCsvFilePath(string userId, int id);
        List<Dataset> GetAllDatasets(string userId);
        List<DimensionValue> GetAllDimensionValues(int datasetId);
        List<DimensionValue> GetAllDimValues(int dimensionId);
        List<Measure> GetAllMeasures(int id);
        Dataset GetDataset(string userId, string name);
        Dataset GetDataset(int id);
        MiningTask GetMiningTask(string userId, int id);
        MiningTask GetMiningTask(string userId, string name);
        List<Dimension> GetChildDimensions(int id);
        Dimension GetDimension(int id);
        Measure GetMeasure(int id);
        AssociationRule GetRule(int id);
    }

    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly IDbContext _dbContext;

        public DataAccessLayer(IDbContext dbContext)
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

        public void ChangeDatasetState(int id, State state)
        {
            var dataset = _dbContext.Datasets.Single(d => d.Id == id);
            dataset.State = state;
            _dbContext.SaveChanges();
        }

        public void SetTaskState(string userId, int taskId, int state, string failedReason = null)
        {
            var task = GetMiningTask(userId, taskId);
            task.TaskState = state;
            task.FailedReason = failedReason;
            _dbContext.SaveChanges();
        }

        public void SetPreprocessed(int datasetId)
        {
            var dataset = GetDataset(datasetId);
            dataset.Preprocessed = true;
            _dbContext.SaveChanges();
        }

        public string GetCsvFilePath(string userId, int id)
        {
            return GetDataset(id).CsvFilePath;
        }

        public List<Dataset> GetAllDatasets(string userId)
        {
            return _dbContext.Datasets.Where(d => d.UserId == userId).ToList();
        }

        public List<DimensionValue> GetAllDimensionValues(int datasetId)
        {
            return _dbContext.DimensionValues.Where(dv => dv.Dimension.DataSet.Id == datasetId).ToList();
        }

        public List<DimensionValue> GetAllDimValues(int dimensionId)
        {
            return _dbContext.DimensionValues.Where(dv => dv.Dimension.Id == dimensionId).ToList();
        }

        public List<Measure> GetAllMeasures(int id)
        {
            return _dbContext.Measures.Where(dv => dv.DataSet.Id == id).ToList();
        }

        public Dataset GetDataset(string userId, string name)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Name == name && d.UserId == userId);
        }

        public Dataset GetDataset(int id)
        {
            return _dbContext.Datasets.SingleOrDefault(d => d.Id == id);
        }

        public MiningTask GetMiningTask(string userId, int id)
        {
            return _dbContext.MiningTasks
                .SingleOrDefault(mt => mt.Id == id && mt.DataSet.UserId == userId);
        }

        public MiningTask GetMiningTask(string userId, string name)
        {
            return _dbContext.MiningTasks
                .SingleOrDefault(mt => mt.Name == name && mt.DataSet.UserId == userId);
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

        public AssociationRule GetRule(int id)
        {
            return _dbContext.AssociationRules.Single(ar => ar.Id == id);
        }
    }
}