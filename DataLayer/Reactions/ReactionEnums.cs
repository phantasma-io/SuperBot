using System;

namespace DataLayer{
    public enum ReactionType {
        Text,
        GlobalVariable,
        DictionaryVariable,
        IO
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

    public enum IOReactionModifiers {
        CreateCSV,
        SaveImage
    }
}