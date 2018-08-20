using System;
using System.Collections.Generic;
using MiddlemanLayer;
using DataLayer;
using Newtonsoft.Json;
using System.Net;

/// <summary>
/// TODO
/// implement sub groups in reactions. They will provide chaining of reactions, each sub group will
/// have one reaction chosen. 
/// 
/// Add a field for reaction delay.async Possibly as subgroup modifiers?
/// </summary>

namespace DataLayer{

    public partial class Reaction
    {
        public ReactionOutputMessage React(CommsLayerMessage msg)
        {
            ReactionOutputMessage output = null;
            
            switch(reactionType)
            {
                case ReactionType.Text:
                    output = new ReactionOutputMessage(this);
                    break;

                case ReactionType.DictionaryVariable:
                    ParseDictionaryReaction(msg);
                    break;

                case ReactionType.IO:
                    ParseSpecialReaction(msg);
                    break;
            }

            return output;
        }

        private void ParseSpecialReaction(CommsLayerMessage msg)
        {
            switch(modifier)
            {
                case (int) IOReactionModifiers.CreateCSV:
                    var csvMetadata = JsonConvert.DeserializeObject<CsvMetadataDBO>(data.ToString());
                    UserVariablesSingleton.DumpToCSV(csvMetadata);
                    break;

                case (int) IOReactionModifiers.SaveImage:
                    var fileMetadata = (Telegram.Bot.Types.File) msg.message;
                    
                    var imageMetadata = JsonConvert.DeserializeObject<ImageMetadataDBO>(data.ToString());

                    FileManager.DownloadAndSaveFile(fileMetadata.FilePath, $"{msg.senderId}{imageMetadata.suffix}.jpg", imageMetadata.subfolderName);
                    break;
            }
        }

        private void ParseDictionaryReaction(CommsLayerMessage clm)
        {
            var key = clm.senderId.ToString();
            var dv = JsonConvert.DeserializeObject<VariableDataDBO>(data.ToString());
            switch(modifier)
            {
                case (int) DictionaryVariableReactionModifiers.SavePresetValue:
                    UserVariablesSingleton.SetDictionaryVariableItem(dv.variable, key, dv.value);
                break;

                case (int) DictionaryVariableReactionModifiers.SaveUserMessage:
                    
                    if(clm.type != CommsLayerMessage.Type.Text)
                        return;
                        
                    UserVariablesSingleton.SetDictionaryVariableItem(dv.variable, key, (string) clm.message);
                break;

                case (int) DictionaryVariableReactionModifiers.DeleteEntry:
                    UserVariablesSingleton.DeleteDictionaryVariable(dv.variable, key);
                break;
            }
        }
    }

    public class ReactionSubgroup
    {
        private List<Reaction> reactions;

        static Random rand = new Random();


        public ReactionOutputMessage SelectAndApplyReaction(CommsLayerMessage msg) 
        {
            var reaction = SelectReaction();

            return reaction.React(msg);
        }

        private Reaction SelectReaction()
        {
            var chosenIndex = 0;

            if(reactions.Count > 1)
                chosenIndex = rand.Next(0,reactions.Count - 1);
                
            var reaction = reactions[chosenIndex];

            return reaction;
        }

        public void AddReaction(Reaction reaction)
        {
            if (reactions == null)
                reactions = new List<Reaction>();

            reactions.Add(reaction);
        }
    }

     public partial class ReactionGroup
    {
        public Dictionary<string, ReactionSubgroup> subgroups = null;

        public List<ReactionOutputMessage> React(CommsLayerMessage msg)
        {
            if (subgroups == null)
                AssembleSubgroups();

            var reactionOutputs = new List<ReactionOutputMessage>();

            foreach (var keypair in subgroups)
            {
                var subgroup = keypair.Value;

                var output = subgroup.SelectAndApplyReaction(msg);

                if (output != null)
                    reactionOutputs.Add(output);
            }

            return reactionOutputs;
        }

        /// <summary>
        /// Organizes the reaction list obtained from JSON into each of their respective subgroups, for easier group and subgroup evaluation
        /// </summary>
        private void AssembleSubgroups()
        {
            subgroups = new Dictionary<string, ReactionSubgroup>();

            foreach (var reaction in reactions)
            {
                var key = reaction.subgroup;

                if(key == null)
                    key = Guid.NewGuid().ToString();

                if (subgroups.ContainsKey(key) == false)
                    subgroups.Add(key, new ReactionSubgroup());

                ReactionSubgroup subgroup;
                subgroups.TryGetValue(key, out subgroup);

                subgroup.AddReaction(reaction);
            }
        }
    }
}