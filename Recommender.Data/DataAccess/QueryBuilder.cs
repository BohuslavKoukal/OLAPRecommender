using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Recommender.Common.Constants;
using Recommender.Data.Extensions;
using Recommender.Data.Models;

namespace Recommender.Data.DataAccess
{
    public interface IQueryBuilder
    {
        void CreateTable(string tableName, IEnumerable<Column> columns, IEnumerable<ForeignKey> keys);

        void CreateView(string datasetName, string factTableName, IEnumerable<Dimension> dimensions, IEnumerable<Measure> measures);

        /// <summary>
        ///  Insert columns as single row to table
        /// </summary>
        void Insert(string tableName, IEnumerable<Column> columns, MySqlConnection connection = null);

        /// <summary>
        ///  Insert columns as multiple rows to table
        /// </summary>
        void Insert(string tableName, IEnumerable<IEnumerable<Column>> columns);
        DataTable Select(string tableName, Column selector);
        DataTable Select(string tableName, List<Column> selectors);
        DataTable Select(string tableName, List<List<Column>> selectors);
        int Count(string tableName);
    }

    public class QueryBuilder : IQueryBuilder
    {
        private readonly IDbConnection _dbConnection;
        private const string IdAutoIncrement = "Id INT NOT NULL AUTO_INCREMENT,";
        private const string IdPrimary = "PRIMARY KEY (Id));";
        private const string ForeignKey = "FOREIGN KEY ({0}) REFERENCES {1}(Id), ";

        public QueryBuilder(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void CreateTable(string tableName, IEnumerable<Column> columns, IEnumerable<ForeignKey> keys)
        {
            var keyList = keys.ToList();
            var columnsString = string.Empty;
            var fkColumns = string.Empty;
            var fkReferences = string.Empty;
            columnsString = columns
                .Aggregate(columnsString, (current, name) => current + $"{name.Name} {name.Type}, ");
            fkColumns = keyList
                .Aggregate(fkColumns, (current, name) => current + $"{name.KeyName} INT, ");
            fkReferences = keyList
                .Aggregate(fkReferences, (current, name) => current + string.Format(ForeignKey, name.KeyName, name.Reference));
            var query = $@"CREATE TABLE {tableName} (
                {IdAutoIncrement}
                {columnsString}
                {fkColumns}
                {fkReferences}
                {IdPrimary}";
            
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();
                var command = new MySqlCommand(query, conn);
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void CreateView(string datasetName, string factTableName, IEnumerable<Dimension> dimensions, IEnumerable<Measure> measures)
        {
            var selectClause = "fact.Id AS Id";
            var fromClause = factTableName + " fact";
            foreach (var dimension in dimensions)
            {
                selectClause += $", {dimension.Name}.{Constants.String.Value} AS {dimension.GetNameValue()}";
                fromClause += $" JOIN {dimension.TableName} {dimension.Name} ON ";
                if (dimension.ParentDimension == null)
                {
                    fromClause += $"fact.{dimension.IdName} = {dimension.Name}.Id";
                }
                else
                {
                    fromClause += $"{dimension.ParentDimension.Name}. {dimension.IdName} = {dimension.Name}.Id";
                }
            }
            foreach (var measure in measures)
            {
                selectClause += $", fact.{measure.Name} AS {measure.Name}";
            }
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();
                var command = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = $@"CREATE VIEW {datasetName}View AS SELECT {selectClause} FROM {fromClause}"
                };
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void Insert(string tableName, IEnumerable<Column> columns, MySqlConnection conn = null)
        {
            var columnList = columns.ToList();
            var columnsString = string.Empty;
            var valuesString = string.Empty;
            columnsString = columnList
                .Aggregate(columnsString, (current, column) => current + $"{column.Name}, ");
            valuesString = columnList
                .Aggregate(valuesString, (current, column) => current + $"'{column.Value}', ");
            columnsString = RemoveLastComma(columnsString);
            valuesString = RemoveLastComma(valuesString);
            if (conn == null)
            {
                using (conn = _dbConnection.GetConnection())
                {
                    conn.Open();
                    var command = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = $@"INSERT INTO {tableName}
                    ({columnsString})
                    VALUES ({valuesString});"
                    };
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            else
            {
                var command = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = $@"INSERT INTO {tableName}
                ({columnsString})
                VALUES ({valuesString});"
                };
                command.ExecuteNonQuery();
            }
            
        }

        public void Insert(string tableName, IEnumerable<IEnumerable<Column>> rows)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();
                foreach (var row in rows)
                {
                    Insert(tableName, row, conn);
                }
                conn.Close();
            }
        }

        public DataTable Select(string tableName, Column selector)
        {
            return Select(tableName, new List<Column> {selector});
        }

        // All AND selectors
        public DataTable Select(string tableName, List<Column> selectors)
        {
            string selectorsString = 
                selectors.Aggregate("42=42", (current, selector) => current + $" AND {selector.Name} = '{selector.Value}'");
            var query = $"SELECT * FROM {tableName} WHERE {selectorsString}";
            return Select(query);
        }

        // Inner selectors are OR, they are joined as AND
        public DataTable Select(string tableName, List<List<Column>> selectors)
        {
            var selectorsString = "42=42";
            foreach (var andSelectors in selectors)
            {
                selectorsString += " AND (42=43";
                selectorsString = andSelectors.Aggregate(selectorsString, (current, orSelector) => current + $" OR {orSelector.Name} = '{orSelector.Value}'");
                selectorsString += ")";
            }
            var query = $"SELECT * FROM {tableName} WHERE {selectorsString}";
            return Select(query);
        }

        public int Count(string tableName)
        {
            int ret;
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();
                var command = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = $@"select count(*) from {tableName}"                    
                };
                ret = Convert.ToInt32(command.ExecuteScalar());
                conn.Close();
            }
            return ret;
        }

        private DataTable Select(string query)
        {
            var results = new DataTable();
            using (var conn = _dbConnection.GetConnection())
            {
                var command = new MySqlCommand(query, conn);
                using (var dataAdapter = new MySqlDataAdapter(command))
                    dataAdapter.Fill(results);
            }
            return results;
        }

        private static string RemoveLastComma(string source)
        {
            var place = source.LastIndexOf(", ", StringComparison.Ordinal);
            if (place == -1)
                return source;
            return source.Remove(place, 2);
        }

    }
}