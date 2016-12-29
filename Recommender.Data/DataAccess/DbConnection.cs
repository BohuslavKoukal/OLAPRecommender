using MySql.Data.MySqlClient;
using Recommender.Common;

namespace Recommender.Data.DataAccess
{
    public interface IDbConnection
    {
        MySqlConnection GetConnection();
    }

    public class DbConnection : IDbConnection
    {
        private readonly string _connection;

        public DbConnection(IConfiguration config)
        {
            _connection = config.GetCubeDatabaseConnectionString();
        }

        public DbConnection()
        {
            _connection = string.Empty;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connection);
        }
    }
}