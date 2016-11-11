using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recommender2.Business.Enums;

namespace Recommender2.Business.Helpers
{
    public static class StringExtensions
    {
        public static string ToString(this object originalString, int dataType)
        {
            return dataType == (int) DataType.DateTime 
                ? ((DateTime)originalString).ToString("yyyyMMdd") 
                : originalString.ToString();
        }
    }
}