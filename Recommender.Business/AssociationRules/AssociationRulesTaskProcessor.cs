using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Recommender.Common;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class AssociationRulesTaskProcessor
    {
        
        private readonly LispMinerConnector _lmConnector;
        private readonly IDataAccessLayer _data;

        public AssociationRulesTaskProcessor(LispMinerConnector lmConnector, IDataAccessLayer data)
        {
            _lmConnector = lmConnector;
            _data = data;
        }

        public void SendTask(MiningTask task)
        {
            var pmmlService = new PmmlService();
            var preprocessingPmml = pmmlService.GetPreprocessingPmml(task);
            var miningTaskPmml = pmmlService.GetMiningTaskPmml(task);
            _lmConnector.SendTask(preprocessingPmml, miningTaskPmml);
            Task.Factory.StartNew(() => SaveTaskResults(task.Name, task.DataSet.Id, task.Id));
        }

        private void SaveTaskResults(string taskName, int datasetId, int taskId)
        {
            var resultsFile = string.Empty;
            // Poll LM connect for results
            while (resultsFile == string.Empty)
            {
                Thread.Sleep(5000);
                resultsFile = _lmConnector.GetTaskResultsFile(taskName);
            }
            var pmmlService = new PmmlService(resultsFile);
            var dimensionValues = _data.GetAllDimensionValues(datasetId);
            var measures = _data.GetAllMeasures(datasetId);
            var rules = pmmlService.GetRules(dimensionValues, measures);
            var taskDuration = pmmlService.GetTaskDuration();
            var numberOfVerifications = pmmlService.GetNumberOfVerifications();
            _data.SaveTaskResults(taskId, rules, numberOfVerifications, taskDuration);
        }
    }
}
