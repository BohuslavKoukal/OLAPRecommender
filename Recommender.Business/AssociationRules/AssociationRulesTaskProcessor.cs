using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
        private readonly RulesPruner _pruner;

        public AssociationRulesTaskProcessor(LispMinerConnector lmConnector,
            IDataAccessLayer data, IConfiguration configuration, RulesPruner pruner)
        {
            _lmConnector = lmConnector;
            _data = data;
            _configuration = configuration;
            _pruner = pruner;
        }

        public void SendTask(MiningTask task, List<Discretization> discretizations, List<EquivalencyClass> eqClasses, int rowCount)
        {
            Task.Factory.StartNew(() => SendToLispMinerAsync(task, discretizations, rowCount, eqClasses));
        }

        private void SendToLispMinerAsync(MiningTask task,
            List<Discretization> discretizations, int rowCount, List<EquivalencyClass> eqClasses)
        {
            var service = new PmmlService(_configuration);
            var preprocessed = task.DataSet.Preprocessed;
            if (!preprocessed)
            {
                var preprocessingPmml = service.GetPreprocessingPmml(task, discretizations, rowCount);
                preprocessed = _lmConnector.SendPreprocessing(task, preprocessingPmml);
            }
            if (preprocessed)
            {
                var taskPmml = service.GetTaskPmml(task, eqClasses, rowCount);
                var responseOk = _lmConnector.SendTask(task, taskPmml);
                if (responseOk)
                {
                    SaveTaskResults(task.Name, task.DataSet.Id, task.Id);
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
