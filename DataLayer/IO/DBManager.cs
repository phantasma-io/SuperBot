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

        private const string svTableName = "singlevar_table";
        private const string svNameColumn = "var_name";

        private const string dvTableName = "dictionaryvar_table";
        private const string dictionaryNameColumn = "dictionary_name";
        private const string userIdColumn = "user_id";
        
        private const string valueColumn = "value";

        private static string dbConnectionString => $"server={dbServer};userid={dbUser};password={dbPassword};database={dbSchema}";
        private static MySqlConnection connection = null;

        public static void TryInitDB()
        {
            //create dictionaryVariable sql table
            string query = $@"CREATE TABLE IF NOT EXISTS {dvTableName}(
                            {dictionaryNameColumn} VARCHAR(300) NOT NULL,
                            {userIdColumn} VARCHAR(300) NOT NULL,
                            {valueColumn} VARCHAR(10000) NULL,
                            PRIMARY KEY ({dictionaryNameColumn}, {userIdColumn})
                            ) CHARACTER SET utf8";
                
            RunNonQuery(query);

            //create singleVariable sql table
            query = $@"CREATE TABLE IF NOT EXISTS {svTableName}(
                            {svNameColumn} VARCHAR(300) NOT NULL,
                            {valueColumn} VARCHAR(10000) NULL,
                            PRIMARY KEY ({svNameColumn})
                            ) CHARACTER SET utf8";

            RunNonQuery(query);
            
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

        public static bool DeleteSingleEntry(string name)
        {
            string query = $@"DELETE FROM {svTableName}";
            query += BuildQueryFilter(name, null);
            return RunNonQuery(query);
        }

        public static bool DeleteDictionaryEntry(string dictionaryName, string userId) {
            string query = $@"DELETE FROM {dvTableName}";
            query += BuildQueryFilter(dictionaryName, userId, null);
            return RunNonQuery(query);
        }

        /// <summary>
        /// When a param is null, selects any value of the corresponding column
        /// </summary>
        /// <param name="dictionaryName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> SelectDictionaryVariables(string dictionaryName = null, string userId = null, string value = null) {
            var output = new Dictionary<string, Dictionary<string, string>>();

            string query = $@"SELECT * FROM {dvTableName}";
            query += BuildQueryFilter(dictionaryName, userId, value);
            query+= ";";

            var reader = RunReader(query);

            while(reader != null && reader.Read())
            {
                var dictName = reader[dictionaryNameColumn].ToString();
                var user = reader[userIdColumn].ToString();
                var val = reader[valueColumn].ToString();
                
                if(!output.ContainsKey(dictName))
                    output.Add(dictName, new Dictionary<string, string>());

                output[dictName].Add(user, val);
            }

            CloseConnection();

            return output;           
        }

        public static Dictionary<string, string> SelectSingleVariables(string variableName = null, string value = null)
        {
            var output = new Dictionary<string, string>();

            string query = $@"SELECT * FROM {svTableName}";
            query += BuildQueryFilter(variableName, value);
            query += ";";

            var reader = RunReader(query);

            while(reader != null && reader.Read())
            {
                var varName = reader[svTableName].ToString();
                var val = reader[valueColumn].ToString();

                output.Add(varName, val);
            }

            CloseConnection();

            return output;
        }

        public static int CountDictionaryVariable(string dictionaryName = null, string userId = null, string value = null)
        {
            string query = $@"SELECT Count(*) FROM {dvTableName}";
            query += BuildQueryFilter(dictionaryName, userId, value);
            query += ";";

            return RunScalar(query);
        }

        public static bool InsertSingleEntry(string variableName, string value)
        {
            string query = $@"INSERT INTO {svTableName} ({svNameColumn}, {valueColumn})
                            VALUES ('{variableName}','{value}')";

            return RunNonQuery(query);
        }

        public static bool InsertDictionaryEntry(string dictionaryName, string userId, string value) 
        {
            string query = $@"INSERT INTO {dvTableName} ({dictionaryNameColumn}, {userIdColumn}, {valueColumn})
                            VALUES ('{dictionaryName}','{userId}','{value}')";

            return RunNonQuery(query);
        }

        public static bool UpdateSingleEntry(string variableName, string value)
        {
            string query = $@"UPDATE {dvTableName} SET {valueColumn}='{value}'";
            query += BuildQueryFilter(variableName, value);
            query+= ";";

            return RunNonQuery(query);
        }

        public static bool UpdateDictionaryEntry(string dictionaryName, string userId, string value)
        {
            string query = $@"UPDATE {dvTableName} SET {valueColumn}='{value}'";
            query += BuildQueryFilter(dictionaryName, userId, null);
            query+= ";";

            return RunNonQuery(query);
        }

        private static bool RunNonQuery(string query)
        {
            try
            {
                OpenConnection();
    
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

        private static MySqlDataReader RunReader(string query)
        {
            try
            {
                OpenConnection();

                var cmd = new MySqlCommand(query,connection);
                var reader = cmd.ExecuteReader();

                //CloseConnection();    //connection must be closed manually after reader is used, or it won't be readable
                return reader;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}",  ex.ToString());
                CloseConnection();
                return null;
            }
        }

        private static int RunScalar(string query)
        {
            int count = 0;
            try
            {
                OpenConnection();

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

        
        private static string BuildQueryFilter(string singleVariableName = null, string value = null)
        {
            string output = "";
            List<string> conditions = new List<string>();

            if (singleVariableName != null || value != null)
            {
                output += " WHERE ";

                if (singleVariableName != null)
                    conditions.Add($@"{svNameColumn}='{singleVariableName}'");

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

    }
}