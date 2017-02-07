using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Recommender.Common.Constants;

namespace Recommender.Data.Models
{
    public class Dimension
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public virtual Dataset DataSet { get; set; }
        public virtual Dimension ParentDimension { get; set; }
        public virtual ICollection<DimensionValue> DimensionValues { get; set; }

        public string TableName => DataSet.Name + Name;
        public string IdName => Name + Constants.String.Id;
        public string FactTableName => DataSet.Name + Constants.String.FactTable;
    }
}