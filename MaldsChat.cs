using System;
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
                    Conversations[update.Message.Chat.Id.ToString()] = GetOpenAiClient().Chat.CreateConversation();

                    await telegramClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: $"New conversation created. Please ask your question.",
                        replyToMessageId: update.Message.MessageId
                    );
                }
                else
                {
                    if (!Conversations.TryGetValue(update.Message.Chat.Id.ToString(), out var chat))
                    {
                        chat = GetOpenAiClient().Chat.CreateConversation();
                        Conversations[update.Message.Chat.Id.ToString()] = chat;
                    }

                    chat.AppendUserInputWithName(update.Message.From.FirstName, update.Message.Text);

                    var responseObject = await chat.GetResponseFromChatbot();

                    string responseText = responseObject.ToString();

                    await telegramClient.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: $"{responseText}",
                        parseMode: ParseMode.Markdown,
                        replyToMessageId: update.Message.MessageId
                        );
                }
            }
            return new OkResult();
        }

        private static TelegramBotClient GetTelegramBotClient()
        {
            var token = Environment.GetEnvironmentVariable("telegramToken");

            if (token is null)
            {
                throw new ArgumentException("Token not found. Please set token as environment variable");
            }

            var telegramClient = new TelegramBotClient(token);

            return telegramClient;
        }

        private static OpenAIAPI GetOpenAiClient()
        {
            APIAuthentication OPENAI_KEY = Environment.GetEnvironmentVariable("openAiApiKey");
            OpenAIAPI api = new OpenAIAPI(OPENAI_KEY);
            return api;
        }
    }
}
