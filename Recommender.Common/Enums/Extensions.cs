using System;

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
                return "FLOAT";
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

}