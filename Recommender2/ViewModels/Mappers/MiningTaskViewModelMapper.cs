using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Markup;
using Recommender.Business.DTO;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender2.ViewModels.Mappers
{

    public interface IMiningTaskViewModelMapper
    {
        MiningTaskViewModel Map(MiningTask task);
        MiningViewModel Map(Dataset dataset, List<CommensurabilityViewModel> commensurabilities);
        List<CommensurabilityViewModel> GetCommensurableDimensions(DimensionTree tree);
        MiningTask Map(MiningViewModel model, Dataset dataset, List<Dimension> commensurableDimensions);
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
                TaskStartTime = task.TaskStartTime,
                TaskState = (TaskState) task.TaskState,
                FailedReason = task.FailedReason,
                Base = task.Base
            };
        }

        public MiningViewModel Map(Dataset dataset, List<CommensurabilityViewModel> commensurabilities)
        {
            return new MiningViewModel
            {
                BaseQ = 1,
                CommensurabilityList = commensurabilities,
                ConditionRequired = false,
                Id = dataset.Id,
                Lift = 2,
                Name = dataset.Name
            };
        }

        public List<CommensurabilityViewModel> GetCommensurableDimensions(DimensionTree tree)
        {
            var ret = new List<CommensurabilityViewModel>();
            foreach (var root in tree.RootDimensions)
            {
                ret.AddRange(root.GetSubtreeDimensionDtos().Select(d => new CommensurabilityViewModel
                {
                    Dimension = new DimensionViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Type = d.Type.ToString()
                    },
                    Checked = tree.IsLeaf(d.Id)
                }));
            }
            return ret;
        }

        public MiningTask Map(MiningViewModel model, Dataset dataset, List<Dimension> commensurableDimensions)
        {
            return new MiningTask
            {
                AssociationRules = new List<AssociationRule>(),
                DataSet = dataset,
                Name = model.TaskName,
                ConditionRequired = model.ConditionRequired,
                NumberOfVerifications = 0,
                TaskDuration = TimeSpan.Zero,
                TaskStartTime = DateTime.Now,
                TaskState = (int)TaskState.Started,
                ConditionDimensions = commensurableDimensions,
                Aad = model.Lift - 1,
                Base = model.BaseQ
            };
        }

        private List<AssociationRuleViewModel> Map(IEnumerable<AssociationRule> rules)
        {
            return rules.Select(rule => new AssociationRuleViewModel
            {
                Id = rule.Id,
                AntecedentValues = Map(rule.AntecedentValues.ToList()),
                ConditionValues = Map(rule.ConditionValues.ToList()),
                Succedents = Map(rule.Succedents.ToList())
            }).ToList();
        }

        private List<string> Map(IEnumerable<DimensionValue> values)
        {
            return values.Select(value => $"{value.Dimension.Name} ({value.Value})").ToList();
        }

        private List<SuccedentViewModel> Map(IEnumerable<Succedent> succedents)
        {
            return succedents.Select(suc => new SuccedentViewModel
            {
                Aad = suc.Aad,
                Base = suc.Base,
                Text = suc.SuccedentText
            }).ToList();
        }
    }
}