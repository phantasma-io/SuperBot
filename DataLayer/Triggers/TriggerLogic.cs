using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MiddlemanLayer;

namespace DataLayer
{

    public partial class Trigger
    {
        public bool Eval(TriggerInputData msg)
        {
            return ParseTriggerInput(msg);
        }

        private bool ParseTriggerInput(TriggerInputData msg)
        {
            switch(msg.type)
            {
                case TriggerType.Text:
                    string text = (string) msg.data;
                    return ParseTextTrigger(text);
                
                default:
                    return false;
            }
        }

        private bool ParseTextTrigger(string text)
        {
            //var query = Regex.Unescape(value);
            var query = value;
            Match match = Regex.Match(text, query);

            return match.Success;
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
        public bool Eval(TriggerInputData msg)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.Eval(msg))
                    return true;
            }
            return false;
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
        public bool Eval(TriggerInputData msg)
        {
            if (subgroups == null)
                AssembleSubgroups();

            foreach (var keypair in subgroups)
            {
                var subgroup = keypair.Value;

                if (!subgroup.Eval(msg))
                    return false;
            }
            return true;
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

                if (subgroups.ContainsKey(key) == false)
                    subgroups.Add(key, new TriggerSubgroup());

                TriggerSubgroup subgroup;
                subgroups.TryGetValue(key, out subgroup);

                subgroup.AddTrigger(trigger);
            }
        }
    }

}