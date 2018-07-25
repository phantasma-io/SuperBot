using System;

namespace DataLayer{
    public enum ReactionType {
        Text,
        GlobalVariable,
        DictionaryVariable,
        Special
    }

    public enum TextReactionModifiers {
        ReplyOnSameChat,
        ReplyOnPublicChat
    }

    public enum GlobalVariableReactionModifiers {
        Default
    }

    public enum DictionaryVariableReactionModifiers {
        SavePresetValue,
        SaveUserMessage,
        DeleteEntry
    }

    public enum SpecialReactionModifiers {
        CreateCSV
    }
}