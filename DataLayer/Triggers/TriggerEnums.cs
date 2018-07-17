using System;

namespace DataLayer {
    public enum TriggerType {
        /// <summary>
        /// This trigger can only be triggered by a text message that matches at least /// once with a Regex query.
        /// </summary>
        Text,
        GlobalVariable,
        DictionaryVariable
    }

    public enum TextTriggerModifiers {
        Contains
    }

    public enum GlobalVariableTriggerModifiers {
        Default
    }

    public enum DictionaryVariableTriggerModifiers {
        MatchValues
    }
}