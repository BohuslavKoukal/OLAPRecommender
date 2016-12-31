using System;
using System.IO;
using Recommender.Common.Enums;

namespace Recommender.Common.Helpers
{
    public static class TypeExtensions
    {
        public static string ToString(this object originalString, Type dataType)
        {
            return dataType == typeof(DateTime) 
                ? ((DateTime)originalString).ToString("yyyyMMdd") 
                : originalString.ToString();
        }

        public static Type ToType(this string originalString)
        {
            if (originalString.Equals(typeof(string).Name))
                return typeof(string);
            if (originalString.Equals(typeof(int).Name))
                return typeof(int);
            if (originalString.Equals(typeof(double).Name))
                return typeof(double);
            if (originalString.Equals(typeof(DateTime).Name))
                return typeof(DateTime);
            throw new InvalidDataException($"{originalString} is unsupported type.");
        }

        public static int ToInt(this Type type)
        {
            if (type == typeof(string))
                return 2;
            if (type == typeof(int))
                return 0;
            if (type == typeof(double))
                return 1;
            if (type == typeof(DateTime))
                return 3;
            throw new InvalidDataException($"{type.Name} is unsupported type.");
        }

        public static Type ToType(this int integer)
        {
            if (integer == 2)
                return typeof(string);
            if (integer == 0)
                return typeof(int);
            if (integer == 1)
                return typeof(double);
            if (integer == 3)
                return typeof(DateTime);
            throw new InvalidDataException($"{integer} is unsupported type.");
        }

        public static string ToUserFriendlyName(this Type type)
        {
            if (type == typeof(string))
                return "Text";
            if (type == typeof(int))
                return "Integer";
            if (type == typeof(double))
                return "Double";
            if (type == typeof(DateTime))
                return "Date";
            return "Unknown type";
        }

    }
}