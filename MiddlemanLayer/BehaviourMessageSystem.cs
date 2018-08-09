using System;
using System.Collections.Generic;
using DataLayer;

namespace MiddlemanLayer {
    /// <summary>
    /// This class represents the full message that the CommsLayer needs to fill in order to fully
    /// allow all possible triggers to be evaluated.
    /// 
    /// As platform agnostic as possible (i.e. telegram, discord, etc.)
    /// </summary>
    public class CommsLayerMessage {
        
        public enum Type{Text, Image};

        readonly public object message;
        readonly public Type type;
        readonly public long senderId;
        readonly public long chatId;

        public CommsLayerMessage(object  msg, Type type, long senderId, long chatId){
            this.message= msg;
            this.type = type;
            this.senderId = senderId;
            this.chatId = chatId;
        }
    }

    public class TriggerOutputMessage {
        public string triggerName {get; private set;}
        public string onFailMsg {get; private set;}
        public bool result {get; private set;}

        public TriggerOutputMessage(string name, TriggerOutput output)
        {
            triggerName = name;
            result = output.result;
            onFailMsg = output.onFailMsg;
        }

        public void Edit(string name, TriggerOutput output)
        {
            triggerName = name;
            result = output.result;
            onFailMsg = output.onFailMsg;
        }
    }
    
    public class ReactionOutputMessage {
        
        public readonly bool replyToPublic;

        readonly public string message;

        public ReactionOutputMessage(Reaction reaction){
            
            if(reaction.reactionType != ReactionType.Text)
                throw new Exception("Tried to create a ReactionOutputMessage from a non-text Reaction");

            this.message = (string) reaction.data;

            if(reaction.modifier == (int) TextReactionModifiers.ReplyOnPublicChat)
                this.replyToPublic = true;
            else
                this.replyToPublic = false;
        }
    }

}