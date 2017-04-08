using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Business;
using Recommender.Business.AssociationRules;
using Recommender.Business.DTO;
using Recommender.Business.GraphService;
using Recommender.Business.StarSchema;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Extensions;
using Recommender.Data.Models;
using Recommender.Web.ViewModels;
using Recommender.Web.ViewModels.Mappers;

namespace Recommender.Web.ControllerEngine
{
    public class MinedResultsControllerEngine : ControllerEngineBase
    {
        private readonly AssociationRulesTaskProcessor _arTaskProcessor;
        private readonly IMiningTaskViewModelMapper _taskMapper;
        private readonly DataDiscretizator _discretizator;
        private readonly IDimensionTreeBuilder _treeBuilder;

        public MinedResultsControllerEngine(IDataAccessLayer data,
            IStarSchemaQuerier starSchemaQuerier,
            AssociationRulesTaskProcessor arTaskProcessor,
            IMiningTaskViewModelMapper taskMapper,
            DataDiscretizator discretizator,
            IDimensionTreeBuilder treeBuilder)
            : base(data, starSchemaQuerier)
        {
            _arTaskProcessor = arTaskProcessor;
            _taskMapper = taskMapper;
            _discretizator = discretizator;
            _treeBuilder = treeBuilder;
        }

        public MiningViewModel GetMiningViewModel(int id)
        {
            return _taskMapper.Map(Data.GetDataset(id),
                GetCommensurableDimensions(_treeBuilder.ConvertToTree(id, false)));
        }

        public void MineRules(int datasetId, MiningViewModel model)
        {
            var dataset = Data.GetDataset(datasetId);
            var commensurableDimensions = model.CommensurabilityList
                .Where(cd => cd.Checked)
                .Select(cd => Data.GetDimension(cd.Dimension.Id)).ToList();
            var task = _taskMapper.Map(model, dataset, commensurableDimensions);
            Data.Insert(task);
            var rowCount = StarSchemaQuerier.GetFactTableRowCount(dataset.GetViewName());
            var discretizations = _discretizator.GetDiscretizations(dataset, rowCount);
            var dimensionTree = _treeBuilder.ConvertToTree(datasetId, true);
            var equivalencyClasses = _discretizator.GetEquivalencyClasses(dimensionTree);
            _arTaskProcessor.SendTask(task, discretizations, equivalencyClasses, rowCount);
        }

        public MiningTaskViewModel GetDetails(int taskId)
        {
            var task = Data.GetMiningTask(taskId);
            return _taskMapper.Map(task);
        }

        public List<CommensurabilityViewModel> GetCommensurableDimensions(DimensionTree tree)
        {
            return _taskMapper.GetCommensurableDimensions(tree);
        }

    }
}