﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Common.Enums;

namespace Recommender.Data.Models
{
    public class MiningTask
    {
        public MiningTask(string name)
        {
            Name = name;
        }

        public MiningTask()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfVerifications { get; set; }
        public int TaskState { get; set; }
        public double Base { get; set; }
        public double Aad { get; set; }
        public DateTime TaskStartTime { get; set; }
        public TimeSpan TaskDuration { get; set; }
        public virtual ICollection<AssociationRule> AssociationRules { get; set; }
        //public virtual ICollection<DimensionValue> Filters { get; set; }

        public virtual Dataset DataSet { get; set; }
    }
}