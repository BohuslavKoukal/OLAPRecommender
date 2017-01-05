using System.Collections.Generic;
using Recommender.Common.Enums;

namespace Recommender.Data.Models
{
    public class RecommenderFile
    {
        public string FilePath { get; set; }
        public virtual ICollection<Attribute> Attributes { get; set; }
    }
}