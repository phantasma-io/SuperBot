using System;
using DataLayer;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using System.Linq;

namespace MiddlemanLayer {
    public class BehaviourManager
    {
        Dictionary<string, TriggerGroup> triggerGroups = null;
        Dictionary<string, ReactionGroup> reactionGroups = null;


        /// <summary>
        /// Evaluates trigger groups from first to last until it finds the first match.
        /// </summary>
        /// <param name="msg">The TriggerInputMessage with all possible infos triggers could need</param>
        /// <returns>Returns the corresponding Behaviour key in case of match, empty string otherwise</returns>
        public TriggerOutputMessage EvaluateTriggers(CommsLayerMessage msg)
        {
            if(triggerGroups == null)
                LoadBehaviours();

            //var firstFailedGroup = new TriggerOutputMessage(null, null);
            TriggerOutputMessage firstFailMsg = null;
            
            foreach(var keypair in triggerGroups)
            {
                var triggerGroup = keypair.Value;

                var groupOutput = triggerGroup.Eval(msg);
                
                if(groupOutput.result)
                    return new TriggerOutputMessage(keypair.Key, groupOutput);
                
                else if(firstFailMsg == null || (firstFailMsg != null && firstFailMsg.onFailMsg == null))
                    firstFailMsg = new TriggerOutputMessage(keypair.Key, groupOutput);
            }
            return firstFailMsg;
        }

        /// <summary>
        /// Applies 1 random reaction per sub-group. All the applied reactions of <ReactionType.Text> will return
        /// a line for the bot to say, and these lines get added to the ReactionOutputMessage.
        /// </summary>
        /// <param name="behaviourKey"></param>
        /// <returns></returns>
        public List<ReactionOutputMessage> ApplyReactions(string behaviourKey, CommsLayerMessage message)
        {
            if(behaviourKey == "" || !reactionGroups.ContainsKey(behaviourKey))
                return new List<ReactionOutputMessage>();

            return reactionGroups[behaviourKey].React(message);
        }

        public void LoadBehaviours()
        {
            LoadTriggers();
            LoadReactions();
            LoadUserVariables();
        }

        private void LoadTriggers()
        {
            string json = FileManager.ReadJsonToString(FileManager.Location.Triggers);
            triggerGroups = JsonConvert.DeserializeObject<Dictionary<string,TriggerGroup>>(json);
        }

        private void LoadReactions()
        {
            string json = FileManager.ReadJsonToString(FileManager.Location.Reactions);
            reactionGroups = JsonConvert.DeserializeObject<Dictionary<string,ReactionGroup>>(json);
        }

        private void LoadUserVariables()
        {
            string json = FileManager.ReadJsonToString(FileManager.Location.UserVariablesList);
            var variables = JsonConvert.DeserializeObject<Dictionary<string, List<UserVariableDBO>>>(json);

            UserVariablesSingleton.LoadUserVariables(variables.ElementAt(0).Value);
        }
    }
}