using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HomeControlFunctions.Functions
{
    public class SabahudinTelegramBot
    {
        private readonly TelegramBotClient _botClient;

        public SabahudinTelegramBot(IConfiguration configuration)
        {
            var telegramApiKey = configuration.GetSection("SabahudinTelegramApiKey").Value;
            _botClient = new TelegramBotClient(telegramApiKey);
        }

        private const string SetUpFunctionName = "setupsabahudin";
        private const string UpdateFunctionName = "handleupdatesabahudin";

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
            text: GetSabahudinsAnswer(update?.Message?.Chat?.FirstName, update?.Message?.Chat?.LastName, update.Message.Text));
        }

        private string GetSabahudinsAnswer(string? senderFirstName, string? senderLastName, string messageText)
        {
            var random = new Random();
            var randomNumer = random.NextInt64(1, 20);
            var responseName = string.IsNullOrEmpty(senderFirstName) ? senderLastName : (senderFirstName ?? "curaz");
            var isQuestion = messageText.Contains("?");

            if (isQuestion)
            {
                randomNumer = 0;
            }

            return randomNumer switch
            {
                0 => "I nix versteh dojc",
                1 => "Hau lele i abe rykshmerc",
                2 => "Prrcc..",
                3 => $"{responseName} ash patat?",
                4 => "Pil ehrs",
                5 => $"Ash epfel {responseName}? Bruhe ajne",
                6 => "Vom wo chomsch tu?",
                7 => "Mehtesh?",
                8 => "Kajn",
                9 => "Sjer",
                10 => "Bini im kjhe",
                11 => "Wan ajne esel isch im stall vom ajne kuh. Tjze dan ajne kuh oder ajne esel?",
                12 => $"Frelihe wajnahte {responseName}",
                13 => "Chomt bald santaclos?",
                14 => "Pushi curaz minj rykc...",
                15 => $"Wj isch tinj name showjder? Tje {responseName} rihtik?",
                16 => "I nix versteh",
                17 => $"oo {responseName} tu bisch ajne gute man",
                _ => "muss"
            };
        }
    }
}
