using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Data.Models
{
    public class DimensionValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public virtual Dimension Dimension { get; set; }
        public string Value { get; set; }
        public ICollection<AssociationRule> AntecedentRules { get; set; }
        public ICollection<AssociationRule> ConditionRules { get; set; }
    }
}
