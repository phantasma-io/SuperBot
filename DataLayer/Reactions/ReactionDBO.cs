using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Reaction-related Data Binding Objects, used for serialization and deserialization of the Reactions.JSON
/// 
/// *** Do not change these unless you are going to change the JSON structure in Reactions.json!! ***
/// 
/// If you are looking to change or add to the behaviours of reactions, that should be done on TriggerLogic.cs
/// </summary>

namespace DataLayer{
    public partial class ReactionGroup {
        public List<Reaction> reactions;
    }

    public partial class Reaction
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReactionType reactionType {get; set;}
        public object value {get;set;}

        public int modifier {get;set;}
        public string subgroup {get;set;}
    }

    //for the csv file reaction
    public partial class CsvMetadataDBO {
        public bool areDictionaries { get;set; }
        public List<string> columns { get; set; }
        public string filename { get; set; }
    }
}