using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Common;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public interface IAssociationRulesTaskProcessor
    {
        void SendTask(string userId, MiningTask task, List<Discretization> discretizations, List<EquivalencyClass> eqClasses,
            int rowCount);
    }

    public class AssociationRulesTaskProcessor : IAssociationRulesTaskProcessor
    {
        private readonly LispMinerConnector _lmConnector;
        private readonly IDataAccessLayer _data;
        private readonly IConfiguration _configuration;
        private readonly RulesPruner _pruner;

        public AssociationRulesTaskProcessor(LispMinerConnector lmConnector,
            IDataAccessLayer data, IConfiguration configuration, RulesPruner pruner)
        {
            _lmConnector = lmConnector;
            _data = data;
            _configuration = configuration;
            _pruner = pruner;
        }

        public void SendTask(string userId, MiningTask task, List<Discretization> discretizations, List<EquivalencyClass> eqClasses, int rowCount)
        {
            Task.Factory.StartNew(() => SendToLispMinerAsync(userId, task, discretizations, rowCount, eqClasses));
        }

        private void SendToLispMinerAsync(string userId, MiningTask task,
            List<Discretization> discretizations, int rowCount, List<EquivalencyClass> eqClasses)
        {
            var service = new PmmlService(_configuration);
            var minerId = task.DataSet.MinerId;
            bool? preprocessed = null;
            if (minerId == null)
            {
                preprocessed = false;
                var minerIdResponse = _lmConnector.GetMinerId(userId, task);
                var newMinerId = PmmlService.GetMinerId(minerIdResponse);
                if (newMinerId == null)
                {
                    _data.SetTaskState(userId, task.Id, (int) TaskState.Failed,
                        "OLAP Recommender was unable to obtain miner id from LISP Miner. Response: " + minerIdResponse);
                    return;
                }
                else
                {
                    task.DataSet.MinerId = newMinerId;
                    _data.SetMinerId(userId, task.DataSet.Id, newMinerId);
                }
            }
            if (preprocessed == null)
            {
                preprocessed = task.DataSet.Preprocessed;
            }
            var preprocessingPmml = service.GetPreprocessingPmml(task, discretizations, rowCount);
            if ((bool) !preprocessed)
            {
                preprocessed = _lmConnector.SendPreprocessing(userId, task, preprocessingPmml);
            }
            if ((bool) preprocessed)
            {
                var taskPmml = service.GetTaskPmml(task, eqClasses, rowCount);
                while (true)
                {
                    var responseOk = _lmConnector.SendTask(userId, task, taskPmml, preprocessingPmml);
                    if (responseOk)
                    {
                        var resultsFile = _lmConnector.GetTaskResultsFile(task.Name);
                        var resultService = new PmmlService(_configuration, resultsFile);
                        var taskState = resultService.GetTaskState();
                        if (taskState == TaskState.Started)
                        {
                            Thread.Sleep(TimeSpan.FromMinutes(2));
                        }
                        else if (taskState == TaskState.Finished)
                        {
                            _data.SetTaskState(userId, task.Id, (int) TaskState.Finished);
                            SaveTaskResults(task.Name, task.DataSet.Id, task.Id);
                            break;
                        }
                        else if (taskState == TaskState.Interrupted)
                        {
                            _data.SetTaskState(userId, task.Id, (int)TaskState.Interrupted, "Hypotheses count too high, define higher Lift or Base to obtain less rules.");
                            SaveTaskResults(task.Name, task.DataSet.Id, task.Id);
                            break;
                        }
                        else
                        {
                            _data.SetTaskState(userId, task.Id, (int)TaskState.Failed, "Task failed due to an unknown reason in Lisp Miner.");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                
            }
        }

        private void SaveTaskResults(string taskName, int datasetId, int taskId)
        {
            var resultsFile = _lmConnector.GetTaskResultsFile(taskName);
            var pmmlService = new PmmlService(_configuration, resultsFile);
            var dimensionValues = _data.GetAllDimensionValues(datasetId);
            var measures = _data.GetAllMeasures(datasetId);
            var rules = pmmlService.GetRules(dimensionValues, measures);
            var postprocessedRules = _pruner.PruneRules(rules);
            var taskDuration = pmmlService.GetTaskDuration();
            var numberOfVerifications = pmmlService.GetNumberOfVerifications();
            _data.SaveTaskResults(taskId, postprocessedRules, numberOfVerifications, taskDuration);
        }


    }
}
