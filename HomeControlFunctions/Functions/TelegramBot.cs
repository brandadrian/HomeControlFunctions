using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HomeControlFunctions.Functions
{
    public class SetupBot
    {
        private readonly TelegramBotClient _botClient;

        public SetupBot(IConfiguration configuration)
        {
            var telegramApiKey = configuration.GetSection("TelegramApiKey").Value;
            _botClient = new TelegramBotClient(telegramApiKey);
        }

        private const string SetUpFunctionName = "setup";
        private const string UpdateFunctionName = "handleupdate";

        [FunctionName(SetUpFunctionName)]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var handleUpdateFunctionUrl = req.GetDisplayUrl().Replace(SetUpFunctionName, UpdateFunctionName,
                                                ignoreCase: true, culture: CultureInfo.InvariantCulture);
            await _botClient.SetWebhookAsync(handleUpdateFunctionUrl);
        }

        [FunctionName(UpdateFunctionName)]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            var request = await req.ReadAsStringAsync();
            var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(request);

            if (update.Type != UpdateType.Message)
                return;
            if (update.Message!.Type != MessageType.Text)
                return;

            await _botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: GetBotResponseForInput(update.Message.Text));
        }

        private string GetBotResponseForInput(string text)
        {
            try
            {
                if (text.Contains("pod bay doors", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "I'm sorry Miszu, I'm afraid I can't do that 🛰";
                }

                return new DataTable().Compute(text, null).ToString();
            }
            catch
            {
                return $"Dear human, I can solve math for you, try '2 + 2 * 3' 👀";
            }
        }
    }
}
