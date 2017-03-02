using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Data.Models
{
    public class Succedent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string SuccedentText { get; set; }
        public double Aad { get; set; }
        public double Base { get; set; }
        public virtual Measure Measure { get; set; }

        [Required]
        public virtual AssociationRule AssociationRule { get; set; }
    }
}
