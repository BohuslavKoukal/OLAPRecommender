using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Models
{
    public class CsvFile
    {
        public string FilePath { get; set; }
        public virtual ICollection<Attribute> Attributes { get; set; }
    }
}