using System;
using DataLayer;
using MiddlemanLayer;
using Microsoft.CSharp.RuntimeBinder;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace CommsLayer
{
    public partial class TelegramListener
    {
        private TelegramBotClient Bot;
        private BehaviourManager behaviourManager = new BehaviourManager();
        private readonly string botToken;

/// <summary>
/// Tries to initialize the telegram listener.
/// </summary>
/// <returns>True on success, false otherwise</returns>
        public TelegramListener(dynamic settings)
        {
            botToken = settings.telegramBotToken;
            Bot = new TelegramBotClient(botToken);

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Bot.StartReceiving();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }
/* */
        private async void BotOnMessageReceived(object sender, MessageEventArgs args)
        {
            try
            {
                var telegramMessage = args.Message;
                var userId = telegramMessage.From.Id;
                var chatId = telegramMessage.Chat.Id;

                object message = null;
                CommsLayerMessage.Type messageType = 0;
                
                string fileId = "";

                if (telegramMessage == null) return;

                switch (telegramMessage.Type)
                {
                    /*case MessageType.ChatMembersAdded:
                        {
                            if (EAT_JOIN_MSGS)
                            {
                                await ReactNewMember(message);
                            }
                            break;
                        }*/

                    case MessageType.Text:
                        message = telegramMessage.Text;
                        messageType = CommsLayerMessage.Type.Text;
                        
                    break;

                    case MessageType.Photo:
                        var i = telegramMessage.Photo.Length - 1;
                        fileId = telegramMessage.Photo[i].FileId;

                        message = (object) (await Bot.GetFileAsync(fileId));
                        ((Telegram.Bot.Types.File)message).FilePath = $"https://api.telegram.org/file/bot{botToken}/{((Telegram.Bot.Types.File)message).FilePath}";
                        messageType = CommsLayerMessage.Type.Image;
                        
                        break;

                    case MessageType.Document:
                        switch (telegramMessage.Document.MimeType)
                        {
                            case "image/png":
                            case "image/jpg":
                            case "image/jpeg":
                                fileId = telegramMessage.Document.FileId;
                                
                                message = (object) await Bot.GetFileAsync(fileId);
                                ((Telegram.Bot.Types.File)message).FilePath = $"https://api.telegram.org/file/bot{botToken}/{((Telegram.Bot.Types.File)message).FilePath}";
                                messageType = CommsLayerMessage.Type.Image;
                            break;
                        }
                        break;
                }

                if(message == null)
                    return;

                bool isPrivateChat = telegramMessage.Chat.Type == ChatType.Private;

                var commsMsg = new CommsLayerMessage(message, messageType, userId, chatId, isPrivateChat);
                var triggerOutput = behaviourManager.EvaluateTriggers(commsMsg);

                if(triggerOutput.result)
                {
                    Console.WriteLine($"{telegramMessage.From.Username}: {triggerOutput.triggerName}");      

                    var reactionOutputMessages = behaviourManager.ApplyReactions(triggerOutput.triggerName, commsMsg);

                    if(reactionOutputMessages != null && reactionOutputMessages.Count > 0)
                    {
                        TypeMessagesAsync(chatId, reactionOutputMessages);
                    }
                }
                else
                {
                    if(triggerOutput.onFailMsg != null)
                        TypeMessageAsync(chatId, triggerOutput.onFailMsg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{args.Message.From.Username}: {e.Message}");
            }
        }

        private async Task TypeMessagesAsync(long chatId, List<ReactionOutputMessage> reactionOutputMessages)
        {
            foreach(ReactionOutputMessage output in reactionOutputMessages)
            {
                string msg = output.message;
                
                if(output.replyToPublic)
                    chatId = 0; //* GET THE PUBLIC CHAT ID FROM THE CONFIG FILE OR SOMETHING */
                
                await TypeMessageAsync(chatId, msg);
            }
        }

        private async Task TypeMessageAsync(long chatId, string reply)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
            await Task.Delay(1000); // simulate longer running task
            await Bot.SendTextMessageAsync(chatId, reply);
        }

        /// <summary>
        /// Have the bot simulate typing a message, and have him actually send it afterwards.
        /// </summary>
        /// <param name="message">The trigger message</param>
        /// <param name="output">A list of the reply output - each entry will be sent in separate messages</param>
        /// <returns></returns>
        /*
        private static async Task TypeMessage(ReactionOutputMessage reaction)
        {
            if (output.Any())
            {
                foreach (var entry in output)
                {
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await Task.Delay(message.Chat.Type == ChatType.Private ? 1000 : 2500); // simulate longer running task
                    await Bot.SendTextMessageAsync(message.Chat.Id, entry);
                }

                lastPublicChat = DateTime.UtcNow;
            }
        }*/
    }
}