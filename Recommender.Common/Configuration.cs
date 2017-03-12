using System.Configuration;
using System.Web.Configuration;

namespace Recommender.Common
{
    public interface IConfiguration
    {
        string GetFilesLocation();
        string GetPmmlFilesLocation();
        string GetMinerId();
        string GetCubeDatabaseConnectionString();
        string GetLispMinerServer();
    }

    public class Configuration : IConfiguration
    {
        public string GetFilesLocation()
        {
            return WebConfigurationManager.AppSettings["FilesLocation"];
        }

        public string GetPmmlFilesLocation()
        {
            return WebConfigurationManager.AppSettings["PmmlFilesLocation"];
        }

        public string GetMinerId()
        {
            return WebConfigurationManager.AppSettings["MinerId"];
        }

        public string GetCubeDatabaseConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CubeDatabase"].ConnectionString;
        }

        public string GetLispMinerServer()
        {
            return WebConfigurationManager.AppSettings["LispMinerAddress"];
        }
    }
}