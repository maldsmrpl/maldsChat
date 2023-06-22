using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using OpenAI_API;
using System.Text;
using OpenAI_API.Chat;

namespace MaldsChat
{
    public static class MaldsChat
    {
        private static Dictionary<string, Conversation> Conversations = new Dictionary<string, Conversation>();

        [FunctionName("maldsChat")]
        public static async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var telegramClient = GetTelegramBotClient();

            var body = await req.ReadAsStringAsync();
            var update = JsonConvert.DeserializeObject<Update>(body);

            if (update.Type == UpdateType.Message)
            {
                string userName = update.Message.From.FirstName;

                if (update.Message.Text.Contains("/start"))
                {
                    await telegramClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: $"Hello {update.Message.From.FirstName} {update.Message.From.LastName}!",
                    replyToMessageId: update.Message.MessageId
                    );
                }
                else if (update.Message.Text.Contains("/new"))
                {
                    // Create a new conversation and store it in the dictionary
                    Conversations[userName] = GetOpenAiClient().Chat.CreateConversation();

                    await telegramClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: $"New conversation created. Please ask your question.",
                        parseMode: ParseMode.MarkdownV2,
                        replyToMessageId: update.Message.MessageId
                        );
                }
                else
                {
                    // If there's an ongoing conversation, append the user's message to it
                    if (Conversations.TryGetValue(userName, out var conversation))
                    {
                        conversation.AppendUserInputWithName(userName, update.Message.Text);
                        var responseObject = await conversation.GetResponseFromChatbotAsync();
                        string responseText = await conversation.GetResponseFromChatbotAsync();

                        await telegramClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: $"{responseText}",
                            parseMode: ParseMode.MarkdownV2,
                            replyToMessageId: update.Message.MessageId
                            );
                    }
                    else
                    {
                        // If there's no ongoing conversation, ask the user to start a new one
                        await telegramClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: $"Please start a new conversation with the /new command.",
                            parseMode: ParseMode.MarkdownV2,
                            replyToMessageId: update.Message.MessageId
                            );
                    }
                }
            }
            return new OkResult();
        }

        private static TelegramBotClient GetTelegramBotClient()
        {
            var token = Environment.GetEnvironmentVariable("telegramToken");

            if (token is null)
            {
                throw new ArgumentException("Token not found. Please set tokenin the Environment Variables.");
            }

            return new TelegramBotClient(token);
        }

        private static OpenAIAPI GetOpenAiClient()
        {
            var apiKey = Environment.GetEnvironmentVariable("openAiToken");

            if (apiKey is null)
            {
                throw new ArgumentException("API Key not found. Please set API Key in the Environment Variables.");
            }

            return new OpenAIAPI(apiKey);
        }
    }
}
