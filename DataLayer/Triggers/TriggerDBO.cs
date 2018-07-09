using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Trigger-related Data Binding Objects, used for serialization and deserialization of the Triggers.JSON
/// 
/// *** Do not change these unless you are going to change the JSON structure in Triggers.json!! ***
/// 
/// If you are looking to change or add to the behaviours of triggers, that should be done on TriggerLogic.cs
/// </summary>

namespace DataLayer
{
     public partial class TriggerGroup
    {
        public bool privateTrigger;
        public List<Trigger> triggers;
    }


    public partial class Trigger
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TriggerType triggerType {get;set;}
        public string value {get;set;}
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TriggerModifiers modifier {get;set;}
        public string subgroup {get;set;}        
    }
}
