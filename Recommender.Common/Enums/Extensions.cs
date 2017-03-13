using System;
using System.Data;

namespace Recommender.Common.Enums
{
    public static class AttributeRoleExtensions
    {
        public static string ToString(this AttributeRole attributeRole)
        {
            switch (attributeRole)
            {
                case AttributeRole.Dimension:
                    return "Dimension";
                case AttributeRole.Measure:
                    return "Measure";
                default:
                    return "Unknown type";
            }
        }
    }

    public static class DataTypeExtensions
    {
        public static string ToSqlType(this Type dataType)
        {
            if (dataType == typeof(int) )
            {
                return "INT";
            }
            else if (dataType == typeof(double))
            {
                return "DECIMAL (32,4)";
            }
            else if (dataType == typeof(DateTime))
            {
                return "DATE";
            }
            else if (dataType == typeof(string))
            {
                return "VARCHAR(1024)";
            }
            return "Unknown type";
        }

    }

    public static class TaskStateExtensions
    {
        public static string ToUserFriendlyString(this TaskState state)
        {
            switch (state)
            {
                case TaskState.Started:
                    return "Started - running";
                case TaskState.Finished:
                    return "Finished";
                 case TaskState.Failed:
                    return "Failed";
            }
            return "Unknown state";
        }

        public static string GetColor(this TaskState state)
        {
            switch (state)
            {
                case TaskState.Started:
                    return "black";
                case TaskState.Finished:
                    return "green";
                case TaskState.Failed:
                    return "red";
            }
            return "black";
        }
    }
}