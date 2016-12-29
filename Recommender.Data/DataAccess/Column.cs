namespace Recommender.Data.DataAccess
{
    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        protected bool Equals(Column other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Value?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}