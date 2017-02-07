using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Data.Models;

namespace Recommender.Data.Extensions
{
    public static class MiningTaskExtension
    {
        public static List<Dimension> GetAntecedentDimensions(this MiningTask task)
        {
            return task.DataSet.Dimensions.Where(dim => task.DataSet.Dimensions.Any(d => d.ParentDimension?.Id == dim.Id)).ToList();
        }

        public static List<Dimension> GetConditionDimensions(this MiningTask task)
        {
            return task.DataSet.Dimensions.Where(dim => task.DataSet.Dimensions.All(d => d.ParentDimension?.Id != dim.Id)).ToList();
        }

        public static string GetAntecedentId(this MiningTask task)
        {
            return "Derived_FTCedentD_Antecedent";
        }

        public static string GetSuccedentId(this MiningTask task)
        {
            return "Derived_FTCedentD_Succedent";
        }

        public static string GetConditionId(this MiningTask task)
        {
            return "Derived_FTCedentD_Condition";
        }

        public static string GetAntecedentBagId(this MiningTask task)
        {
            return "Derived_FTCedentBagD_Antecedent";
        }

        public static string GetSuccedentBagId(this MiningTask task)
        {
            return "Derived_FTCedentBagD_Succedent";
        }

        public static string GetConditionBagId(this MiningTask task)
        {
            return "Derived_FTCedentBagD_Condition";
        }
    }
}
