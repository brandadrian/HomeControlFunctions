using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HomeControlFunctions.Services;
using HomeControlFunctions.Models;

namespace HomeControlFunctions.Functions
{
    public class OcrFunctions
    {
        private readonly OcrService _ocrService;

        public OcrFunctions(OcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [FunctionName("GetTextFromImageV2")]
        public async Task<IActionResult> GetTextFromImageV2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request received.");

            var lines = new List<string>();
            var response = string.Empty;

            try
            {
                var fileBytes = Array.Empty<byte>();
                var formData = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                lines = await _ocrService.GetTextFromImage(fileBytes);
                response = "Successful";
            }
            catch (Exception e)
            {
                response = e.Message;
            }

            return new OkObjectResult(new TextFromImageResponseDto(response, lines));
        }

        [FunctionName("GetTextFromImage")]
        public async Task<IActionResult> GetTextFromImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request received.");

            var lines = new List<string>();
            var response = string.Empty;

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                var base64File = data?.base64File.ToString();
                var fileBytes = Convert.FromBase64String(base64File);

                lines = await _ocrService.GetTextFromImage(fileBytes);
                response = "Successful";
            }
            catch (Exception e)
            {
                response = e.Message;
            }

            return new OkObjectResult(new TextFromImageResponseDto(response, lines));
        }
    }
}
