using System.Configuration;
using System.Web.Configuration;

namespace Recommender.Common
{
    public interface IConfiguration
    {
        string GetFilesLocation();
        string GetCubeDatabaseConnectionString();
    }

    public class Configuration : IConfiguration
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