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

        public MinedResultsControllerEngine(IDataAccessLayer data,
            IStarSchemaQuerier starSchemaQuerier,
            SubcubeService subcubeService,
            AssociationRulesTaskProcessor arTaskProcessor,
            IMiningTaskViewModelMapper taskMapper)
            : base(data, starSchemaQuerier)
        {
            _subcubeService = subcubeService;
            _arTaskProcessor = arTaskProcessor;
            _taskMapper = taskMapper;
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
            _arTaskProcessor.SendTask(task);
        }

        public MiningTaskViewModel GetDetails(int taskId)
        {
            var task = Data.GetMiningTask(taskId);
            return _taskMapper.Map(task);
        }

    }
}