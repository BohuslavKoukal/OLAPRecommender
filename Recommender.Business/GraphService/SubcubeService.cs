using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Data.DataAccess;

namespace Recommender.Business.GraphService
{
    public class SubcubeService
    {
        private readonly IDataAccessLayer _data;
        public SubcubeService(IDataAccessLayer data)
        {
            _data = data;
        }

        
    }
}
