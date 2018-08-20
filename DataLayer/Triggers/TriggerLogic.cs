using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using MiddlemanLayer;
using Newtonsoft.Json;

namespace DataLayer
{
    public partial class TriggerOutput {
        public bool result {get; private set;}
        public string onFailMsg {get; private set;}

        public TriggerOutput(bool result, string onFailMsg)
        {
            this.result = result;
            this.onFailMsg = onFailMsg;
        }
    }

    public partial class Trigger
    {
        public TriggerOutput Eval(CommsLayerMessage msg)
        {
            return new TriggerOutput(ParseTriggerInput(msg), onFailMsg);
        }

        private bool ParseTriggerInput(CommsLayerMessage msg)
        {
            switch(triggerType)
            {
                case TriggerType.Text:
                    return ParseTextTrigger(msg);

                case TriggerType.DictionaryVariable:                     
                    return ParseDictionaryTrigger(msg);

                case TriggerType.SingleVariable:
                    return ParseVariableTrigger(msg);

                case TriggerType.Image:
                    return ParseImageTrigger(msg);

                default:
                    return false;
            }
        }

        private bool ParseImageTrigger(CommsLayerMessage msg)
        {
            if(msg.type != CommsLayerMessage.Type.Image)
                return false;

            switch(modifier)
            {
                case 0:
                    var triggerData = JsonConvert.DeserializeObject<ImageTriggerMetadataDBO>(data.ToString());
                    var fileMetadata = (Telegram.Bot.Types.File) msg.message;
                    var fileSize = fileMetadata.FileSize / (1024.0f*1024.0f);

                    bool result = true;

                    if(triggerData.minSize.HasValue && fileSize < triggerData.minSize.Value)
                        result = false;

                    if(triggerData.maxSize.HasValue && fileSize > triggerData.maxSize.Value)
                        result = false;

                    return result;
                default:
                    return false;
            }
        }

        private bool ParseVariableTrigger(CommsLayerMessage msg)
        {
            var dv = JsonConvert.DeserializeObject<VariableDataDBO>(data.ToString());
            string variableName, value;

            switch(modifier)
            {
                case (int) SingleVariableTriggerModifiers.MatchPresetToValue:
                    variableName = dv.variable;
                    value = dv.value;

                    return UserVariablesSingleton.DoesSingleVariableMatch(variableName, value);

                case (int) SingleVariableTriggerModifiers.MatchMessageToValue:
                    if(msg.type != CommsLayerMessage.Type.Text)
                        return false;

                    variableName = dv.variable;
                    value = msg.message.ToString();

                    return UserVariablesSingleton.DoesSingleVariableMatch(variableName, value);

                default:
                    return false;
            }
        }

        private bool ParseDictionaryTrigger(CommsLayerMessage msg)
        {
            var dv = JsonConvert.DeserializeObject<VariableDataDBO>(data.ToString());
            string dictionaryName, userId, value;

            switch(modifier)
            {
                case (int) DictionaryVariableTriggerModifiers.MatchMessageToValue:
                    if(msg.type != CommsLayerMessage.Type.Text)
                        return false;

                    dictionaryName = dv.variable;
                    userId = msg.senderId.ToString();
                    value = msg.message.ToString();

                    return UserVariablesSingleton.DoesDictionaryVariableMatch(dictionaryName, userId, value);

                case (int) DictionaryVariableTriggerModifiers.MatchPresetToValue:
                    dictionaryName = dv.variable;
                    userId = msg.senderId.ToString();
                    value = dv.value;

                    return UserVariablesSingleton.DoesDictionaryVariableMatch(dictionaryName, userId, value);

                case (int) DictionaryVariableTriggerModifiers.KeyExists:
                    dictionaryName = dv.variable;
                    userId = msg.senderId.ToString();

                    return UserVariablesSingleton.DoesDictionaryVariableUserExist(dictionaryName, userId);

                default:
                    return false;
            }
        }

        private bool ParseTextTrigger(CommsLayerMessage msg)
        {
            if(msg.type != CommsLayerMessage.Type.Text)
                return false;

            switch(modifier)
            {
                case (int) TextTriggerModifiers.Contains:

                    string text = (string) msg.message;
                    string query = (string) data;

                    Match match = Regex.Match(text, query);

                    return match.Success;
                
                default:
                    return false;
            }
        }
    }

    public class TriggerSubgroup
    {
        private List<Trigger> triggers;

        /// <summary>
        /// If any <Trigger> is <true>, then the <TriggerSubgroup> evaluates as <true>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public TriggerOutput Eval(CommsLayerMessage msg)
        {
            var subgroupOutput = new TriggerOutput(true, null);

            foreach (var trigger in triggers)
            {
                var triggerOutput = trigger.Eval(msg);

                if (triggerOutput.result)
                    return triggerOutput;
                else
                    subgroupOutput = triggerOutput;
            }
            return subgroupOutput;
        }

        public void AddTrigger(Trigger trigger)
        {
            if (triggers == null)
                triggers = new List<Trigger>();

            triggers.Add(trigger);
        }
    }

     public partial class TriggerGroup
    {

        public Dictionary<string, TriggerSubgroup> subgroups = null;

        /// <summary>
        /// If any <TriggerSubgroup> is false, then the <TriggerGroup> evaluates as false
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public TriggerOutput Eval(CommsLayerMessage msg)
        {
            if (subgroups == null)
                AssembleSubgroups();

            if(msg.isPrivateChat != privateTrigger)
                return new TriggerOutput(false, null);

            var groupOutput = new TriggerOutput(true, null);

            foreach (var keypair in subgroups)
            {
                var subgroup = keypair.Value;
                var subgroupOutput = subgroup.Eval(msg);
                
                if (subgroupOutput.result == false)
                    return subgroupOutput;
            }
            return groupOutput;
        }

        /// <summary>
        /// Organizes the trigger list obtained from JSON into each of their respective subgroups, for easier group and subgroup evaluation
        /// </summary>
        private void AssembleSubgroups()
        {
            subgroups = new Dictionary<string, TriggerSubgroup>();

            foreach (var trigger in triggers)
            {
                var key = trigger.subgroup;

                if(key == null)
                    key = Guid.NewGuid().ToString();

                if (subgroups.ContainsKey(key) == false)
                    subgroups.Add(key, new TriggerSubgroup());

                TriggerSubgroup subgroup;
                subgroups.TryGetValue(key, out subgroup);

                subgroup.AddTrigger(trigger);
            }
        }
    }
}