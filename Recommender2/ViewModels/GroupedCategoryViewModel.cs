using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Recommender2.ViewModels
{
    public class GroupedCategoryViewModel
    {
        public string Name { get; set; }

        //[JsonIgnore]
        //[ScriptIgnore]
        //public int Id { get; set; }
        // Category id, category value
        public List<GroupedCategoryViewModel> Categories { get; set; }
    }

}