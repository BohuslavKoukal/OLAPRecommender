using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recommender.Common.Constants;
using Recommender.Common.Enums;

namespace Recommender.Data.Models
{
    public class Dataset
    {
        public Dataset(string name)
        {
            Name = name;
        }

        public Dataset()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string CsvFilePath { get; set; }
        public State State { get; set; }

        public virtual ICollection<Attribute> Attributes { get; set; }
        public virtual ICollection<Dimension> Dimensions { get; set; }
        public virtual ICollection<Measure> Measures { get; set; }
        public virtual ICollection<MiningTask> MiningTasks { get; set; }

        
    }
}