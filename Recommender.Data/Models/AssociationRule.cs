using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Data.Models
{
    public class AssociationRule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        
        public string Text { get; set; }
        public virtual ICollection<DimensionValue> AntecedentValues { get; set; }
        public virtual ICollection<DimensionValue> ConditionValues { get; set; }
        public virtual ICollection<Succedent> Succedents { get; set; }
        public virtual MiningTask MiningTask { get; set; }
    }
}
