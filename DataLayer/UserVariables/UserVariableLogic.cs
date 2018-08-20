using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataLayer{

    public class DictionaryVariable {
        public Dictionary<string, string> items;
        public bool saveToDisk;

        public DictionaryVariable(bool saveToDisk){
            this.saveToDisk = saveToDisk;
            items = new Dictionary<string, string>();
        }
    }

    public class SingleVariable {
        public string name;
        public string value;
        public bool saveToDisk;
    }

    //make this thread safe please
    public static class UserVariablesSingleton {
        static Dictionary<string, DictionaryVariable> dictionaryVariables = null;
        static Dictionary<string, string> singleVariables = null;
        
        public static void LoadUserVariables(List<UserVariableDBO> variables){
            dictionaryVariables = new Dictionary<string, DictionaryVariable>();
            singleVariables = new Dictionary<string, string>();

            DBManager.TryInitDB();

            var dbSingleVariables = DBManager.SelectSingleVariables(null, null);

            foreach(var variable in variables) {
                if(variable.isDictionary)
                {
                    dictionaryVariables.Add(variable.name, new DictionaryVariable(variable.saveToDisk));
                    
                    var dbOutput = DBManager.SelectDictionaryVariables(variable.name);

                    if(!dbOutput.ContainsKey(variable.name))
                        continue;

                    foreach(var entry in dbOutput[variable.name])
                        dictionaryVariables[variable.name].items.Add(entry.Key, entry.Value);
                }
                    
                else
                {
                    string value = variable.value;
                    //if the single variable exists in the DB and it is currently set for disk interaction, load the value from there
                    if(variable.saveToDisk){
                        if(dbSingleVariables.ContainsKey(variable.name))
                            value = dbSingleVariables[variable.name];
                        else
                            DBManager.InsertSingleEntry(variable.name, variable.value);
                    }
                    else if(dbSingleVariables.ContainsKey(variable.name))   //if the saveToDisk value changed in JSON and we previously saved the variable to DB, erase it now
                        DBManager.DeleteSingleEntry(variable.name);

                    singleVariables.Add(variable.name, value);
                }
                    
            }
        }

        public static bool DoesSingleVariableExist(string variableName)
        {
            return singleVariables.ContainsKey(variableName);
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

        public static bool DoesSingleVariableMatch(string variableName, string value)
        {
            return DoesSingleVariableExist(variableName) && singleVariables[variableName] == value;
        }

        public static bool DoesDictionaryVariableMatch(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                return false;

            return dictionaryVariables[dictionaryName].items[userId] == value;
        }

        public static void SetSingleVariable(string variableName, string value)
        {
            if(DoesSingleVariableExist(variableName) == false)  //single variables can only be created on runtime, they have to be defined first on the UserVariablesList.json
                return;

            UpdateSingleVariable(variableName, value);
        }

        public static void SetDictionaryVariableItem(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                AddDictionaryVariableItem(dictionaryName, userId, value);
            else
                UpdateDictionaryVariable(dictionaryName, userId, value);
            
            return;
        }

        private static void AddDictionaryVariableItem(string dictionaryName, string userId, string value = null)
        {
            DictionaryVariable dictionary = dictionaryVariables[dictionaryName];

            value = value == null ? "" : value;
            
            dictionary.items.TryAdd(userId, value);

            DBManager.InsertDictionaryEntry(dictionaryName, userId, value);
        }

        private static void UpdateSingleVariable(string name, string value)
        {
            singleVariables[name] = value;

            if(DBManager.SelectSingleVariables(name).Count != 0)
                DBManager.UpdateSingleEntry(name, value);
        }

        private static void UpdateDictionaryVariable(string dictionaryName, string userId, string value)
        {
            if(!DoesDictionaryVariableUserExist(dictionaryName, userId))
                return;

            dictionaryVariables[dictionaryName].items[userId] = value;

            DBManager.UpdateDictionaryEntry(dictionaryName, userId, value);
        }

        public static void DeleteSingleVariable(string name)
        {
            if(!DoesSingleVariableExist(name))
                return;

            singleVariables.Remove(name);

            DBManager.DeleteSingleEntry(name);
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
            string csv;
            switch(meta.areDictionaries)
            {
                case true:
                    csv = "User Id";
                    foreach(var columnName in meta.variableNames)
                    {
                        if(dictionaryVariables.ContainsKey(columnName) == false)
                        {
                            Console.WriteLine("Error: cannot create csv, dictionary name does not exist");
                            return;
                        }
                        csv += $", {columnName}";
                    }
                    
                    csv += "\r\n";

                    var firstDict = dictionaryVariables[meta.variableNames[0]];
                    
                    foreach(var entry in firstDict.items)
                    {
                        string userId = entry.Key;
                        
                        csv += userId;   //user id

                        foreach(var columnName in meta.variableNames)
                            csv += $", {dictionaryVariables[columnName].items[userId]}";
                    }
                    
                    FileManager.SaveToCsv(meta.filename, csv);

                break;
                case false:
                    csv = "Variable name, Value\r\n";
                    foreach(var variableName in meta.variableNames)
                    {
                        if(singleVariables.ContainsKey(variableName) == false)
                            continue;
                        
                        string value = singleVariables[variableName];
                        csv += $@"{variableName},{value}\r\n";
                    }

                    FileManager.SaveToCsv(meta.filename, csv);

                break;
            }
        }
    }
}