using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using Recommender2.Business;

namespace Recommender2.DataAccess
{
    public class DbConnection
    {
        private readonly string _connection;

        public DbConnection(Configuration config)
        {
            _connection = config.GetCubeDatabaseConnectionString();
        }

        public MySqlConnection Connection => new MySqlConnection(_connection);
    }
}