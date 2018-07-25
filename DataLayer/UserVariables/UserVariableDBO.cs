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

    public class DictionaryVariableDBO {
        public string variable;
        public string value;
    }
}