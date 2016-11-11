using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender2.Business.DTO
{
    public class GraphDto
    {
        public string Name { get; set; }
        public List<GraphXAxisRootDto> Roots { get; set; }
    }

    public abstract class GraphXAxisRootDto
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public List<GraphXAxisLeafDto> XAxisLeaves { get; set; }
        public abstract double GetValue();
    }

    public abstract class GraphXAxisLeafDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}