using System;
using System.Collections.Generic;

namespace DataLayer {

    public class UserVariableDBO {
        public string name;
        public bool saveToDisk;
        public bool isDictionary;
        public string value;
    }

    public class UserVariableListDBO {
        List<UserVariableDBO> variables {get; set;}
    }

    //this class only exists to allow for easy deserialization of json into a single dictionary item entry.
    public class VariableDataDBO {
        public string variable;
        public string value;
    }
}