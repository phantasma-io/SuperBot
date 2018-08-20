using System;

namespace DataLayer {
    public enum TriggerType {
        /// <summary>
        /// This trigger can only be triggered by a text message that matches at least /// once with a Regex query.
        /// </summary>
        Text,
        SingleVariable,
        DictionaryVariable,
        Image,
    }

    public enum TextTriggerModifiers {
        Contains
    }

    public enum SingleVariableTriggerModifiers {
        MatchValues,
        VariableExists
    }

    public enum DictionaryVariableTriggerModifiers {
        MatchValues,
        KeyExists
    }

    public enum ImageTriggerModifiers {
        IsImage
    }
}