using System.Collections.Generic;

namespace Recommender.Data.Models
{
    public class CsvFile
    {
        public string FilePath { get; set; }
        public virtual ICollection<Attribute> Attributes { get; set; }
    }
}