using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Constants;
using Recommender.Data.Models;

namespace Recommender.Data.Extensions
{
    public static class DatasetExtension
    {
        public static string GetViewName(this Dataset dataset)
        {
            return dataset.Name + Constants.String.View;
        }
    }
}
