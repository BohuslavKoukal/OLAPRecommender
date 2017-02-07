using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.DTO.Mappers;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;
using Recommender2.ViewModels;
using Recommender2.ViewModels.Mappers;

namespace Recommender2.ControllerEngine
{
    public class MinedResultsControllerEngine : ControllerEngineBase
    {
        private readonly AssociationRulesTaskProcessor _arTaskProcessor;
        private readonly SubcubeService _subcubeService;
        private readonly IMiningTaskViewModelMapper _taskMapper;
        private readonly DataDiscretizator _discretizator;
        private readonly IDimensionTreeBuilder _treeBuilder;

        public MinedResultsControllerEngine(IDataAccessLayer data,
            IStarSchemaQuerier starSchemaQuerier,
            SubcubeService subcubeService,
            AssociationRulesTaskProcessor arTaskProcessor,
            IMiningTaskViewModelMapper taskMapper,
            DataDiscretizator discretizator,
            IDimensionTreeBuilder treeBuilder)
            : base(data, starSchemaQuerier)
        {
            _subcubeService = subcubeService;
            _arTaskProcessor = arTaskProcessor;
            _taskMapper = taskMapper;
            _discretizator = discretizator;
            _treeBuilder = treeBuilder;
        }

        public void MineRules(int datasetId, string name, double baseQ,
            double aadQ, Dictionary<int, Dictionary<int, bool>> dimensions)
        {
            var dataset = Data.GetDataset(datasetId);
            var filters = _subcubeService.GetFilters(dimensions);
            var task = new MiningTask
            {
                AssociationRules = new List<AssociationRule>(),
                DataSet = dataset,
                Name = name,
                NumberOfVerifications = 0,
                TaskDuration = TimeSpan.Zero,
                TaskStartTime = DateTime.Now,
                TaskState = (int) TaskState.Started,
                //Filters = DimensionValueMapper.ConvertToDimensionValues(filters.Select(f => (DimensionDto) f).ToList()).ToList(),
                Aad = aadQ,
                Base = baseQ
            };
            Data.Insert(task);
            var rowCount = StarSchemaQuerier.GetFactTableRowCount(dataset.GetViewName());
            var discretizations = _discretizator.GetDiscretizations(dataset);
            var dimensionTree = _treeBuilder.ConvertToTree(datasetId, true);
            var equivalencyClasses = _discretizator.GetEquivalencyClasses(dimensionTree);
            _arTaskProcessor.SendTask(task, rowCount, discretizations, equivalencyClasses);
        }

        public MiningTaskViewModel GetDetails(int taskId)
        {
            var task = Data.GetMiningTask(taskId);
            return _taskMapper.Map(task);
        }

    }
}