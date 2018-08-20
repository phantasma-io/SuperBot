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
        MatchMessageToValue,
        MatchPresetToValue,     //since variables of this type have to be declared beforehand on the JSON file, makes no sense to add "KeyExists"
    }

    public enum DictionaryVariableTriggerModifiers {
        MatchMessageToValue,
        MatchPresetToValue,
        KeyExists
    }

    public enum ImageTriggerModifiers {
        IsImage
    }
}