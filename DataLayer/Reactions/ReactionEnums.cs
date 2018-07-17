using System;

namespace DataLayer{
    public enum ReactionType {
        Text,
        GlobalVariable,
        DictionaryVariable
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
}