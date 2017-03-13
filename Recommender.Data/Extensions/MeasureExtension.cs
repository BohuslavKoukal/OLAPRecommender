using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Data.Models;

namespace Recommender.Data.Extensions
{
    public static class MeasureExtension
    {
        public static string GetBbaId(this Measure measure)
        {
            return $"Basic_FTLiteralD_{measure.Name}";
        }

        public static string GetDbaId(this Measure measure)
        {
            return $"Derived_FTLiteralD_Sign_{measure.Name}";
        }

        public static string GetQualifiedName(this Measure measure)
        {
            return $"{measure.DataSet.Name}_{measure.Name}_Value";
        }
    }
}
