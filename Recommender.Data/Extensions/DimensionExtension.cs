using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Data.Models;

namespace Recommender.Data.Extensions
{
    public static class DimensionExtension
    {
        public static string GetBbaId(this Dimension dimension)
        {
            return $"Basic_FTLiteralD_{dimension.Name}";
        }

        public static string GetQualifiedNameValue(this Dimension dimension)
        {
            return $"{dimension.DataSet.Name}_{dimension.Name}_Value";
        }

        public static string GetDbaId(this Dimension dimension)
        {
            return $"Derived_FTLiteralD_Sign_{dimension.Name}";
        }
        
    }
}
