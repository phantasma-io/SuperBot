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
        
        readonly public string message;
        readonly public long senderId;
        readonly public long chatId;

        public CommsLayerMessage(string  msg, long senderId, long chatId){
            this.message= msg;
            this.senderId = senderId;
            this.chatId = chatId;
        }
    }
    
    public class ReactionOutputMessage {
        
        public readonly bool replyToPublic;

        readonly public string message;

        public ReactionOutputMessage(Reaction reaction){
            
            if(reaction.reactionType != ReactionType.Text)
                throw new Exception("Tried to create a ReactionOutputMessage from a non-text Reaction");

            this.message = (string) reaction.value;

            if(reaction.modifier == (int) TextReactionModifiers.ReplyOnPublicChat)
                this.replyToPublic = true;
            else
                this.replyToPublic = false;
        }
    }

}