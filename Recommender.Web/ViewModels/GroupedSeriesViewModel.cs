using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender.Web.ViewModels
{
    public class GroupedSeriesViewModel
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public double[] Data { get; set; }
    }
}