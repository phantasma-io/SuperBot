using System;
using DataLayer;
using MiddlemanLayer;
using Microsoft.CSharp.RuntimeBinder;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;

namespace CommsLayer
{
    public partial class TelegramListener
    {
        private TelegramBotClient Bot;
        private BehaviourManager behaviourManager = new BehaviourManager();

/// <summary>
/// Tries to initialize the telegram listener.
/// </summary>
/// <returns>True on success, false otherwise</returns>
        public TelegramListener(dynamic settings)
        {
            string botToken = settings.telegramBotToken;
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
        private void BotOnMessageReceived(object sender, MessageEventArgs args)
        {
            try
            {
                var message = args.Message;
                var userId = message.From.Id;
                var chatId = message.Chat.Id;

                if (message == null) return;

                switch (message.Type)
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
                        var triggerMessage = new TriggerInputData(TriggerType.Text, message.Text, userId, chatId);
                        var matchedTriggerLabel = behaviourManager.EvaluateTriggers(triggerMessage);

                        Console.WriteLine(matchedTriggerLabel);

                        var reaction = behaviourManager.GetReaction(matchedTriggerLabel);

                        if(reaction != null)
                        {
                            reaction.SetReplyDestination(userId, chatId);
                            ParseReaction(reaction);
                        }
                        
                    break;

                    default:
                        {
                            /* if (message.Chat.Type == ChatType.Private)
                            {
                                await PrivateChat(message);
                            }
                            else
                            {
                                await PublicChat(message);
                            } */

                            break;
                        }
                }

                /* var kyc_delta = DateTime.UtcNow - last_kyc_reload;
                if (kyc_delta.TotalMinutes > 5)
                {
                    ReloadKYC();
                } */

            }
            catch (Exception e)
            {
                Console.WriteLine($"{args.Message.From.Username}: {e.Message}");
            }
        }

        private async void ParseReaction(ReactionOutputMessage reaction)
        {
            switch(reaction.type)
            {
                case ReactionType.Text:
                    string msg = (string) reaction.data;
                    await TypeMessage(reaction.chatId, msg);
                break;

                default:
                break;
            }

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

        private async Task TypeMessage(long chatId, string reply)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
            await Task.Delay(1000); // simulate longer running task
            await Bot.SendTextMessageAsync(chatId, reply);
        }
    }
}