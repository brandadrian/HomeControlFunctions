using HomeControlFunctions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControlFunctions.Services
{
    public class OcrService
    {
        private readonly ILogger<OcrService> _logger;
        private readonly string _subscriptionKey;
        private readonly string _clientEndpoint;

        public OcrService(ILogger<OcrService> logger, IOptions<ConfigurationOptions> configuration)
        {
            _logger = logger;
            _subscriptionKey = configuration.Value.CognitiveServicesSubscriptionKey;
            _clientEndpoint = configuration.Value.CognitiveServicesEndpoint;
        }

        public async Task<List<string>> GetTextFromImage(byte[] bytes)
        {
            var client = Authenticate(_clientEndpoint, _subscriptionKey);
            var result = await ExtractText(client, bytes);

            return result;
        }

        private static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            return client;
        }

        private async Task<List<string>> ExtractText(ComputerVisionClient client, byte[] file)
        {
            _logger.LogInformation("Start extracting text");

            using var stream = new MemoryStream(file);
            var textHeaders = await client.ReadInStreamAsync(stream);
            var operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            var operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            // Write to result
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            var result = new List<string>();
            foreach (var page in textUrlFileResults)
            {
                foreach (var line in page.Lines)
                {
                    result.Add(line.Text);
                }
            }

            _logger.LogInformation("Extracting text done");

            return result;
        }
    }
}
