using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Common;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class AssociationRulesTaskProcessor
    {
        private readonly LispMinerConnector _lmConnector;
        private readonly IDataAccessLayer _data;
        private readonly IConfiguration _configuration;

        public AssociationRulesTaskProcessor(LispMinerConnector lmConnector, IDataAccessLayer data, IConfiguration configuration)
        {
            _lmConnector = lmConnector;
            _data = data;
            _configuration = configuration;
        }

        public void SendTask(MiningTask task, int rowCount, List<Discretization> discretizations, List<EquivalencyClass> eqClasses)
        {
            var pmmlService = new PmmlService(_configuration);
            var preprocessingAndTaskPmml = pmmlService.GetPreprocessingAndTaskPmml(task, rowCount, discretizations, eqClasses);
            _lmConnector.SendTask(preprocessingAndTaskPmml);
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
            var pmmlService = new PmmlService(_configuration, resultsFile);
            var dimensionValues = _data.GetAllDimensionValues(datasetId);
            var measures = _data.GetAllMeasures(datasetId);
            var rules = pmmlService.GetRules(dimensionValues, measures);
            var taskDuration = pmmlService.GetTaskDuration();
            var numberOfVerifications = pmmlService.GetNumberOfVerifications();
            _data.SaveTaskResults(taskId, rules, numberOfVerifications, taskDuration);
        }
    }
}
