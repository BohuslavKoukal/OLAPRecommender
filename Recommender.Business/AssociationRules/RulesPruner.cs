using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class RulesPruner
    {
        public List<AssociationRule> PruneRules(List<AssociationRule> allRules)
        {
            var ret = new List<AssociationRule>();
            foreach (var rule in allRules)
            {
                var matchingRule = GetMatchingRule(ret, rule);
                if (matchingRule == null)
                {
                    ret.Add(rule);
                }
                else
                {
                    matchingRule.Succedents.Add(rule.Succedents.Single());
                }
            }
            return ret;
        }

        private AssociationRule GetMatchingRule(List<AssociationRule> rules, AssociationRule rule)
        {
            return rules
                .SingleOrDefault(assocRule => 
                DimensionValuesMatch(assocRule.AntecedentValues.ToList(), rule.AntecedentValues.ToList()) && 
                DimensionValuesMatch(assocRule.ConditionValues.ToList(), rule.ConditionValues.ToList()) && 
                SuccedentMeasuresMatch(assocRule.Succedents.FirstOrDefault(), rule.Succedents.Single()));
        }

        private bool DimensionValuesMatch(List<DimensionValue> firstValues, List<DimensionValue> secondValues)
        {
            if (firstValues.Count != secondValues.Count) return false;
            return firstValues.All(firstDimValue => secondValues.Any(dv => dv.Dimension.Id == firstDimValue.Dimension.Id && dv.Value == firstDimValue.Value));
        }

        private bool SuccedentMeasuresMatch(Succedent first, Succedent second)
        {
            return first.Measure.Id == second.Measure.Id;
        }
    }
}
