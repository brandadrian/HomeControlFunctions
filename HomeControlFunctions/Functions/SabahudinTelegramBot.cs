using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HomeControlFunctions.Functions
{
    public class SabahudinTelegramBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly List<string> _sabahudinTelegramResponses = new List<string>();

        public SabahudinTelegramBot(IConfiguration configuration)
        {
            var telegramApiKey = configuration.GetSection("SabahudinTelegramApiKey").Value;
            _botClient = new TelegramBotClient(telegramApiKey);
            Init(configuration);
        }

        private const string SetUpFunctionName = "setupsabahudin";
        private const string UpdateFunctionName = "handleupdatesabahudin";

        [FunctionName(SetUpFunctionName)]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            try
            {
                var handleUpdateFunctionUrl = req.GetDisplayUrl().Replace(SetUpFunctionName, UpdateFunctionName,
                    ignoreCase: true, culture: CultureInfo.InvariantCulture);
                await _botClient.SetWebhookAsync(handleUpdateFunctionUrl);
            }
            catch (Exception e)
            {
                log.LogError(e, "Error handling request.");
                throw;
            }
        }

        [FunctionName(UpdateFunctionName)]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger log)
        {
            try
            {
                var request = await req.ReadAsStringAsync();

                var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(request);

                var firstName = update?.Message?.Chat?.FirstName;
                var lastName = update?.Message?.Chat?.LastName;
                var text = update?.Message?.Text;
                log.LogInformation("Request received. FirstName: {FirstName}. LastName: {LastName}. Text: {Text}", firstName, lastName, text);

                if (update.Type != UpdateType.Message)
                    return;
                if (update.Message!.Type != MessageType.Text)
                    return;

                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: GetSabahudinsAnswer(firstName, lastName, text));
            }
            catch (Exception e)
            {
                log.LogError(e, "Error handling request.");
                throw;
            }
        }

        private string GetSabahudinsAnswer(string? senderFirstName, string? senderLastName, string messageText)
        {
            try
            {
                var responseName = string.IsNullOrEmpty(senderFirstName) ? senderLastName : (senderFirstName ?? "curaz");

                _sabahudinTelegramResponses.Add($"{responseName} ash patat?");
                _sabahudinTelegramResponses.Add($"Ash epfel {responseName}? Bruhe ajne");
                _sabahudinTelegramResponses.Add($"Frelihe wajnahte {responseName}");
                _sabahudinTelegramResponses.Add($"Wj isch tinj name showjder? Tje {responseName} rihtik?");
                _sabahudinTelegramResponses.Add($"oo {responseName} tu bisch ajne gute man");

                var random = new Random();
                var randomNumer = random.Next(0, _sabahudinTelegramResponses.Count - 1);

                return _sabahudinTelegramResponses[randomNumer];
            }
            catch
            {
                return "I nix versteh";
            }
        }

        private void Init(IConfiguration configuration)
        {
            try
            {
                _sabahudinTelegramResponses.AddRange(configuration.GetSection("SabahudinTelegramResponses").Get<List<string>>());
            }
            catch
            {
                Console.WriteLine("Error loading responses");
                //Ignore
            }
        }
    }
}
