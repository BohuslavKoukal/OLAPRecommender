namespace Recommender.Common.Enums
{
    public enum AttributeRole
    {
        Dimension = 1,
        Measure = 2
    }

    public enum State
    {
        Initial = 0,
        FileUploaded = 1,
        DimensionsAndMeasuresSet = 2
    }

    public enum FileType
    {
        Csv = 1,
        Ttl = 2
    }

}