using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using Recommender2.Business;

namespace Recommender2.DataAccess
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