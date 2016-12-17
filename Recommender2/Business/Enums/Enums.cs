using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Business.Enums
{
    public enum AttributeRole
    {
        Dimension = 1,
        Measure = 2
    }

    public enum DataType
    {
        Integer = 0,
        Double = 1,
        String = 2,
        DateTime = 3
    }

    public enum State
    {
        Initial = 0,
        FileUploaded = 1,
        DimensionsAndMeasuresSet = 2
    }

}