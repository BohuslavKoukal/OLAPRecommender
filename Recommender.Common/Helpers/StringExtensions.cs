using System;
using Recommender.Common.Enums;

namespace Recommender.Common.Helpers
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