using System;
using System.Collections.Generic;

namespace DataLayer {

    public class UserVariable {
        public string name;
        public int saveMethod;
        public bool isDictionary;
        public string startValue;
    }

    public class UserVariableDBO {
        List<UserVariable> variables {get; set;}
    }

    public class DictionaryVariable {
        public Dictionary<string, string> items;
        public int saveMethod;
        public string startValue;

        public DictionaryVariable(string start, int save){
            startValue = start;
            saveMethod = save;
            items = new Dictionary<string, string>();
        }
    }

    public class DictionaryVariableDBO {
        public string variable;
        public string value;
    }
}