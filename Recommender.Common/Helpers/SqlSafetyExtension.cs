using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Recommender.Common.Helpers
{
    public static class SqlSafetyExtension
    {
        public static string SafeName(this string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9_-]");
            var safeStr = rgx.Replace(str, "_");
            return safeStr;
        }

        // Lisp Miner can handle only 50 chars in Attribute name
        public static string ShortName(this string str, string datasetName)
        {
            var stringMaxLength = 49 - "_Value".Length - datasetName.Length;
            if (str.Length <= stringMaxLength)
            {
                return str;
            }
            return str.Remove(stringMaxLength - 1);
        }
    }
}
