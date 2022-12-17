using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using HomeControlFunctions.Services;

namespace HomeControlFunctions.Functions
{
    public class Ocr
    {
        private readonly OcrService _ocrService;

        public Ocr(OcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [FunctionName("GetTextFromImage")]
        public async Task<IActionResult> GetTextFromImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request received.");

            var responseMessage = string.Empty;
            
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                var base64File = data?.base64File.ToString();

                var result = await _ocrService.GetTextFromImage(base64File);

                responseMessage = string.Join("\n", result);
            }
            catch (Exception e)
            {
                responseMessage = e.Message;
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
