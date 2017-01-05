using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recommender.Data.Models
{
    public class Measure
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string RdfUri { get; set; }
        public int Type { get; set; }
        public virtual Dataset DataSet { get; set; }
    }
}