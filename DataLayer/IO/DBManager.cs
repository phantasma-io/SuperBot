using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace DataLayer
{
    public static class DBManager
    {
        private const string dbServer = "localhost";
        private const string dbUser = "root";
        private const string dbPassword = "Alcapone3";
        private const string dbSchema = "botdev";

        private const string dvTableName = "dictionaryvar_table";
        private const string dictionaryNameColumn = "dictionary_name";
        private const string userIdColumn = "user_id";
        private const string valueColumn = "value";

        private static string dbConnectionString => $"server={dbServer};userid={dbUser};password={dbPassword};database={dbSchema}";
        private static MySqlConnection connection = null;

        public static bool TryInitDB()
        {
            try
            {
                OpenConnection();

                //create dictionaryVariable sql table
                string query = $@"CREATE TABLE IF NOT EXISTS {dvTableName}(
                                {dictionaryNameColumn} VARCHAR(100) NOT NULL,
                                {userIdColumn} VARCHAR(100) NOT NULL,
                                {valueColumn} VARCHAR(500) NULL,
                                PRIMARY KEY ({dictionaryNameColumn}, {userIdColumn})
                                )";
                
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                
                CloseConnection();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return false;
            }
            
        }

        /* public static bool SetDictionaryEntry(string dictionaryName, string userId, string value) 
        {
            int count = CountDictionaryVariable(dictionaryName, userId, value);
            if(count > 0)
                return UpdateDictionaryEntry(dictionaryName, userId, value);
            if(count == 0)
                return InsertDictionaryEntry(dictionaryName, userId, value);
            
            return false;
        } */

        public static bool DeleteDictionaryEntry(string dictionaryName, string userId) {
            try
            {
                OpenConnection();
                
                string query = $@"DELETE FROM {dvTableName} WHERE {dictionaryNameColumn}='{dictionaryName} AND {userIdColumn}='{userId};";
                var cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();

                CloseConnection();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return false;
            }
        }

        /// <summary>
        /// When a param is null, selects any value of the corresponding column
        /// </summary>
        /// <param name="dictionaryName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> SelectDictionaryVariables(string dictionaryName = null, string userId = null) {
            var output = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                OpenConnection();

                string query = $@"SELECT * FROM {dvTableName}";
                query += BuildQueryFilter(dictionaryName, userId);
                query+= ";";
                
                var cmd = new MySqlCommand(query,connection);
                var reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    var dictName = reader[dictionaryNameColumn].ToString();
                    var user = reader[userIdColumn].ToString();
                    var value = reader[valueColumn].ToString();
                    
                    if(!output.ContainsKey(dictName))
                        output.Add(dictName, new Dictionary<string, string>());

                    output[dictName].Add(user, value);
                }

                CloseConnection();
                return output;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return output;
            }
        }

        public static int CountDictionaryVariable(string dictionaryName = null, string userId = null, string value = null)
        {
            int count = 0;
            try
            {
                OpenConnection();

                string query = $@"SELECT Count(*) FROM {dvTableName}";
                query += BuildQueryFilter(dictionaryName, userId, value);
                query += ";";

                var cmd = new MySqlCommand(query, connection);
                count = (int) cmd.ExecuteScalar();

                CloseConnection();
                return count;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return -1;
            }
        }

        public static bool InsertDictionaryEntry(string dictionaryName, string userId, string value) 
        {
            try
            {
                OpenConnection();
                
                string query = $@"INSERT INTO {dvTableName} ({dictionaryNameColumn}, {userIdColumn}, {valueColumn})
                                VALUES ('{dictionaryName}','{userId}','{value}')";
    
                var cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                CloseConnection();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return false;
            }
        }

        public static bool UpdateDictionaryEntry(string dictionaryName, string userId, string value)
        {
            try
            {
                OpenConnection();
                
                string query = $@"UPDATE {dvTableName} SET {valueColumn}='{value}'
                                  WHERE {dictionaryNameColumn}='{dictionaryName}' AND {userIdColumn}='{userId}'";
    
                var cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();

                CloseConnection();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return false;
            }
        }

        private static string BuildQueryFilter(string dictionaryName = null, string userId = null, string value = null)
        {
            string output = "";
            List<string> conditions = new List<string>();

            if (dictionaryName != null || userId != null || value != null)
            {
                output += " WHERE ";

                if (dictionaryName != null)
                    conditions.Add($@"{dictionaryNameColumn}='{dictionaryName}'");

                if (userId != null)
                    conditions.Add($@"{userIdColumn}='{userId}'");

                if (value != null)
                    conditions.Add($@"{valueColumn}='{value}'");
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                if (i > 0)
                    output += " AND ";

                output += conditions[i];
            }

            return output;
        }

        private static bool OpenConnection(){
            try
            {
                connection = new MySqlConnection(dbConnectionString);
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                throw;
            }
        }

        private static bool CloseConnection() {
            try
            {
                if(connection.State == ConnectionState.Open)
                    connection.Close();
                
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                throw;
            }
        }

    }
}