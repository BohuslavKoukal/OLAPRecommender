using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Constants;
using Recommender.Common.Helpers;
using Recommender.Data.Models;

namespace Recommender.Data.Extensions
{
    public static class DatasetExtension
    {
        public static string GetViewName(this Dataset dataset)
        {
            return dataset.GetPrefix() + dataset.Name + Constants.String.View;
        }

        public static string GetFactTableName(this Dataset dataset)
        {
            return dataset.GetPrefix() + dataset.Name + Constants.String.FactTable;
        }

        public static List<Dimension> GetNonDateDimensions(this Dataset dataset)
        {
            return dataset.Dimensions.Where(d => d.Type.ToType() != typeof(DateTime)).ToList();
        }

        public static string GetPrefix(this Dataset dataset)
        {
            return dataset.UserId.Substring(0, 8);
        }
    }
}
