using System;
using DataLayer;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

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
        public string EvaluateTriggers(TriggerInputData msg)
        {
            if(triggerGroups == null)
                LoadBehaviours();

            foreach(var keypair in triggerGroups)
            {
                var triggerGroup = keypair.Value;

                if(triggerGroup.Eval(msg))
                    return keypair.Key;
            }
            return "";
        }

        public ReactionOutputMessage GetReaction(string behaviourKey)
        {
            return reactionGroups[behaviourKey].SelectReaction();
        }

        public void LoadBehaviours()
        {
            LoadTriggers();
            LoadReactions();
        }

        private void LoadTriggers()
        {
            string json = DataUtils.ReadJsonToString(DataUtils.Files.Triggers);
            triggerGroups = JsonConvert.DeserializeObject<Dictionary<string,TriggerGroup>>(json);
        }

        private void LoadReactions()
        {
            string json = DataUtils.ReadJsonToString(DataUtils.Files.Reactions);
            reactionGroups = JsonConvert.DeserializeObject<Dictionary<string,ReactionGroup>>(json);
        }

    }
}