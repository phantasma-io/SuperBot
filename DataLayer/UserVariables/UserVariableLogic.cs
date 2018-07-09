using System;
using System.Collections.Generic;

namespace DataLayer{
    //make this thread safe please
    public static class UserVariablesSingleton {
        static Dictionary<string, DictionaryVariable> dictionaryVariables = new Dictionary<string, DictionaryVariable>();
        static Dictionary<string, string> singleVariables = new Dictionary<string, string>();   //save method is instant by default for these types
        
        public static void LoadUserVariables(List<UserVariable> variables){
            dictionaryVariables = new Dictionary<string, DictionaryVariable>();
            singleVariables = new Dictionary<string, string>();

            foreach(var variable in variables) {
                if(variable.isDictionary)
                    dictionaryVariables.Add(variable.name, new DictionaryVariable(variable.startValue, variable.saveMethod));
                else
                    singleVariables.Add(variable.name, variable.startValue);
            }
        }

        public static bool AddKey(string variableName, string key, string value = null)
        {
            if(!dictionaryVariables.ContainsKey(variableName))
                return false;   

            DictionaryVariable dictionary = dictionaryVariables[variableName];

            value = value == null ? dictionary.startValue : value;
            
            return dictionary.items.TryAdd(key, value);
        }

        public static bool UpdateKey(string variableName, string key, string value)
        {
            if(!dictionaryVariables.ContainsKey(variableName))
                return false;   

            DictionaryVariable dictionary = dictionaryVariables[variableName];

            if(!dictionary.items.ContainsKey(key))
                return false;

            dictionary.items[key] = value;

            return true;
        }

        public static bool DeleteKey(string variableName, string key)
        {
            if(!dictionaryVariables.ContainsKey(variableName))
                return false;   

            DictionaryVariable dictionary = dictionaryVariables[variableName];

            if(!dictionary.items.ContainsKey(key))
                return false;

            return dictionary.items.Remove(key);
        }

        private class DictionaryVariable {
            public Dictionary<string, string> items;
            public int saveMethod;
            public string startValue;

            public DictionaryVariable(string start, int save){
                startValue = start;
                saveMethod = save;
            }
        }
    }
}