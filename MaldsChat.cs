using System;
using System.IO;
using System.Threading.Tasks;
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
using System.Linq;
using System.Net.Http;

namespace MaldsChat
{
    public static class MaldsChat
    {
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
                else
                {
                    var chat = GetOpenAiClient().Chat.CreateConversation();
                    chat.AppendUserInput(update.Message.Text);
                    string response = await chat.GetResponseFromChatbot();
                    await telegramClient.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: $"{response}",
                    parseMode: ParseMode.MarkdownV2,
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

        private static string UnicodeString(string text)
        {
            return Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(text));
        }
    }
}
