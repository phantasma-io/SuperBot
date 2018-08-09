using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataLayer{
    //make this thread safe please
    public static class UserVariablesSingleton {
        static Dictionary<string, DictionaryVariable> dictionaryVariables = null;
        static Dictionary<string, string> singleVariables = null;   //save method is instant by default for this type of user variable
        
        public static void LoadUserVariables(List<UserVariable> variables){
            dictionaryVariables = new Dictionary<string, DictionaryVariable>();
            singleVariables = new Dictionary<string, string>();

            DBManager.TryInitDB();

            foreach(var variable in variables) {
                if(variable.isDictionary)
                {
                    dictionaryVariables.Add(variable.name, new DictionaryVariable(variable.startValue, variable.saveMethod));
                    
                    var dbOutput = DBManager.SelectDictionaryVariables(variable.name);

                    if(!dbOutput.ContainsKey(variable.name))
                        continue;

                    foreach(var entry in dbOutput[variable.name])
                        dictionaryVariables[variable.name].items.Add(entry.Key, entry.Value);
                }
                    
                else
                    singleVariables.Add(variable.name, variable.startValue);
            }
        }

        public static bool DoesDictionaryVariableUserExist(string dictionaryName, string userId)
        {
            if(!dictionaryVariables.ContainsKey(dictionaryName))
                return false;   

            DictionaryVariable dictionary = dictionaryVariables[dictionaryName];

            if(!dictionary.items.ContainsKey(userId))
                return false;
            return true;
        }

        public static bool DoesDictionaryVariableMatch(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                return false;

            return dictionaryVariables[dictionaryName].items[userId] == value;
        }

        public static void SetDictionaryVariable(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                AddDictionaryVariable(dictionaryName, userId, value);
            else
                UpdateDictionaryVariable(dictionaryName, userId, value);
            
            return;
        }

        private static void AddDictionaryVariable(string dictionaryName, string userId, string value = null)
        {
            DictionaryVariable dictionary = dictionaryVariables[dictionaryName];

            value = value == null ? dictionary.startValue : value;
            
            dictionary.items.TryAdd(userId, value);

            DBManager.InsertDictionaryEntry(dictionaryName, userId, value);
        }

        private static void UpdateDictionaryVariable(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                return;

            dictionaryVariables[dictionaryName].items[userId] = value;

            DBManager.UpdateDictionaryEntry(dictionaryName, userId, value);
        }

        public static void DeleteDictionaryVariable(string dictionaryName, string userId)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                return;

            dictionaryVariables[dictionaryName].items.Remove(userId);

            DBManager.DeleteDictionaryEntry(dictionaryName, userId);
        }

        public static void DumpToCSV(CsvMetadataDBO meta)
        {
            switch(meta.areDictionaries)
            {
                case true:
                    string csv = "User Id";
                    foreach(var columnName in meta.columns)
                    {
                        if(dictionaryVariables.ContainsKey(columnName) == false)
                        {
                            Console.WriteLine("Error: cannot create csv, dictionary name does not exist");
                            return;
                        }
                        csv += $", {columnName}";
                    }
                    
                    csv += "\r\n";

                    var firstDict = dictionaryVariables[meta.columns[0]];
                    
                    foreach(var entry in firstDict.items)
                    {
                        string userId = entry.Key;
                        
                        csv += userId;   //user id

                        foreach(var columnName in meta.columns)
                            csv += $", {dictionaryVariables[columnName].items[userId]}";
                    }
                    
                    FileManager.SaveToCsv(meta.filename, csv);

                break;
                case false:

                break;
            }
        }
    }
}