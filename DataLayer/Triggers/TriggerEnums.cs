using System;

namespace DataLayer {
    public enum TriggerType {
        /// <summary>
        /// This trigger can only be triggered by a text message that matches at least /// once with a Regex query.
        /// </summary>
        Text
    }

    public enum TriggerModifiers {
        Contains,
        NotContains,
        SmallerThan,
        BiggerThan
    }
}