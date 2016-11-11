using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using Recommender2.Business;
using Recommender2.Models;

namespace Recommender2.DataAccess
{
    public class QueryBuilder
    {
        
        private const string IdAutoIncrement = "Id INT NOT NULL AUTO_INCREMENT,";
        private const string IdPrimary = "PRIMARY KEY (Id));";
        private const string ForeignKey = "FOREIGN KEY ({0}) REFERENCES {1}(Id), ";

        public QueryBuilder()
        {
        }

        public void CreateTable(string tableName, IEnumerable<Column> columns, IEnumerable<ForeignKey> keys, MySqlConnection connection)
        {
            var keyList = keys.ToList();
            var columnsString = string.Empty;
            var fkColumns = string.Empty;
            var fkReferences = string.Empty;
            columnsString = columns
                .Aggregate(columnsString, (current, name) => current + $"{name.Name} {name.Type}, ");
            //RemoveLastComma(columnsString);
            fkColumns = keyList
                .Aggregate(fkColumns, (current, name) => current + $"{name.KeyName} INT, ");
            //RemoveLastComma(fkColumns);
            fkReferences = keyList
                .Aggregate(fkReferences, (current, name) => current + string.Format(ForeignKey, name.KeyName, name.Reference));
            var query = $@"CREATE TABLE {tableName} (
                {IdAutoIncrement}
                {columnsString}
                {fkColumns}
                {fkReferences}
                {IdPrimary}";
            
            using (var command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
                
        }

        public void Insert(string tableName, IEnumerable<Column> columns, MySqlConnection connection)
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
            var tCommand = new MySqlCommand
            {
                Connection = connection,
                CommandText = $@"INSERT INTO {tableName}
                ({columnsString})
                VALUES ({valuesString});"
            };
            tCommand.ExecuteNonQuery();
        }

        //public DataTable Select(string tableName, MySqlConnection connection)
        //{
        //    return Select(tableName, new List<Column>(), connection);
        //}

        public DataTable Select(string tableName, Column selector, MySqlConnection connection)
        {
            return Select(tableName, new List<Column> {selector}, connection);
        }

        // All AND selectors
        public DataTable Select(string tableName, List<Column> selectors, MySqlConnection connection)
        {
            string selectorsString = 
                selectors.Aggregate("42=42", (current, selector) => current + $" AND {selector.Name} = '{selector.Value}'");
            var query = $"SELECT * FROM {tableName} WHERE {selectorsString}";
            return Select(query, connection);
        }

        // Inner selectors are OR, they are joined as AND
        public DataTable Select(string tableName, List<List<Column>> selectors, MySqlConnection connection)
        {
            var selectorsString = "42=42";
            foreach (var andSelectors in selectors)
            {
                selectorsString += " AND (42=43";
                selectorsString = andSelectors.Aggregate(selectorsString, (current, orSelector) => current + $" OR {orSelector.Name} = '{orSelector.Value}'");
                selectorsString += ")";
            }
            var query = $"SELECT * FROM {tableName} WHERE {selectorsString}";
            return Select(query, connection);
        }

        private static DataTable Select(string query, MySqlConnection connection)
        {
            var results = new DataTable();
            using (var command = new MySqlCommand(query, connection))
            using (var dataAdapter = new MySqlDataAdapter(command))
                dataAdapter.Fill(results);
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