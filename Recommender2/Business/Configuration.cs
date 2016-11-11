using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Recommender2.Business
{
    public class Configuration
    {
        public string GetFilesLocation()
        {
            return WebConfigurationManager.AppSettings["FilesLocation"];
        }

        public string GetCubeDatabaseConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CubeDatabase"].ConnectionString;
        }
    }
}