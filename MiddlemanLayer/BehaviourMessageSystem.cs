using System;
using DataLayer;

namespace MiddlemanLayer {
    /// <summary>
    /// This class represents the full message that the CommsLayer needs to fill in order to fully
    /// allow all possible triggers to be evaluated.
    /// 
    /// As platform agnostic as possible (i.e. telegram, discord, etc.)
    /// </summary>
    public class TriggerInputData {
        
        readonly public TriggerType type;
        readonly public object data;
        readonly public long senderId;
        readonly public long chatId;

        public TriggerInputData(TriggerType type, object data, long senderId, long chatId){
            this.type = type;
            this.data = data;
            this.senderId = senderId;
            this.chatId = chatId;
        }
    }
    
    public class ReactionOutputMessage {
        
        public long senderId;
        public long chatId;

        readonly public ReactionType type;
        readonly public object data;

        public ReactionOutputMessage(ReactionType type, object data)
        {
            this.type = type;
            this.data = data;
        }

        public void SetReplyDestination(long sender, long chat)
        {
            senderId = sender;
            chatId = chat;
        }
    }

}