using HomeControlFunctions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<ConfigurationOptions> _configuration;

        public SabahudinTelegramBot(IOptions<ConfigurationOptions> configuration)
        {
            _configuration = configuration;
            var telegramApiKey = _configuration.Value.SabahudinTelegramApiKey;
            _botClient = new TelegramBotClient(telegramApiKey);
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
                    text: GetSabahudinsAnswer(log));
            }
            catch (Exception e)
            {
                log.LogError(e, "Error handling request.");
                throw;
            }
        }

        private string GetSabahudinsAnswer(ILogger log)
        {
            try
            {
                var sabahudinTelegramResponses = new List<string>();

                AddResponsesFromConfig(sabahudinTelegramResponses, log);
                    
                var random = new Random();
                var randomNumer = random.Next(0, sabahudinTelegramResponses.Count - 1);

                return sabahudinTelegramResponses[randomNumer];
            }
            catch
            {
                return "...";
            }
        }

        private void AddResponsesFromConfig(List<string> sabahudinTelegramResponses, ILogger log)
        {
            try
            {
                log.LogInformation("Start add responses from config.");
                var responses = _configuration.Value.SabahudinTelegramResponses.Split(';');
                log.LogInformation("Responses: {Responses}", string.Join(';', responses));
                sabahudinTelegramResponses.AddRange(responses);
                log.LogInformation("Responses added");
            }
            catch (Exception e)
            {
                log.LogError(e, "Error loading responses.");
            }
        }
    }
}
