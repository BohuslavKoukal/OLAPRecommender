using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace Recommender2.Business.Enums
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
        public static string ToString(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Integer:
                    return "Integer";
                case DataType.Double:
                    return "Double";
                case DataType.DateTime:
                    return "DateTime";
                case DataType.String:
                    return "String";
                default:
                    return "Unknown type";
            }
        }

        public static string ToSqlType(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Integer:
                    return "INT";
                case DataType.Double:
                    return "FLOAT";
                case DataType.DateTime:
                    return "DATE";
                case DataType.String:
                    return "VARCHAR(1024)";
                default:
                    return "Unknown type";
            }
        }

    }

}