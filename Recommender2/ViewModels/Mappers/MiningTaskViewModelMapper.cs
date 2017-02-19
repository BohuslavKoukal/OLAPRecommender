using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender2.ViewModels.Mappers
{

    public interface IMiningTaskViewModelMapper
    {
        MiningTaskViewModel Map(MiningTask task);
    }

    public class MiningTaskViewModelMapper : IMiningTaskViewModelMapper
    {
        public MiningTaskViewModel Map(MiningTask task)
        {
            return new MiningTaskViewModel
            {
                Name = task.Name,
                Aad = task.Aad,
                AssociationRules = Map(task.AssociationRules.ToList()),
                Id = task.Id,
                NumberOfVerifications = task.NumberOfVerifications,
                TaskDuration = task.TaskDuration,
                Filters = null,
                TaskStartTime = task.TaskStartTime,
                TaskState = (TaskState) task.TaskState,
                Base = task.Base
            };
        }

        private List<AssociationRuleViewModel> Map(IEnumerable<AssociationRule> rules)
        {
            return rules.Select(rule => new AssociationRuleViewModel
            {
                Id = rule.Id,
                AssociationRuleText = rule.Text,
                Aad = rule.Aad,
                Base = rule.Base
            }).ToList();
        }
    }
}