using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HomeControlFunctions.Models;
using Newtonsoft.Json;
using HomeControlFunctions.Services;

namespace HomeControlFunctions.Functions
{
    public class GasRecordFunctions
    {
        private readonly OcrService _ocrService;

        public GasRecordFunctions(OcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [FunctionName("UploadGasRecordFromFile")]
        public async Task<IActionResult> UploadGasRecordFromFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Sql("dbo.GasRecords", ConnectionStringSetting = "HomeControlSqlConnection")] IAsyncCollector<GasRecordDao> gasRecords,
            ILogger log)
        {
            log.LogInformation("UploadGasRecordFromFile request received.");

            var fileBytes = Array.Empty<byte>();
            var formData = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var lines = await _ocrService.GetTextFromImage(fileBytes);
            var firstLine = lines.FirstOrDefault();
            var value = firstLine?.Replace(" ", string.Empty).Trim();
            var hasGasRecordValue = int.TryParse(value, out var gasRecordValue);
            var image = Convert.ToBase64String(fileBytes);

            await gasRecords.AddAsync(new GasRecordDao()
            {
                Value = hasGasRecordValue ? gasRecordValue : null,
                ValueRaw = string.Join(";", lines),
                Timestamp = DateTime.UtcNow,
                Image = image
            });
            await gasRecords.FlushAsync();

            log.LogInformation($"Successfully inserted record. Lines: {lines.Count}. Values: {string.Join(';', lines)}");

            return new OkResult();
        }

        [FunctionName("InsertGasRecord")]
        public async Task<IActionResult> InsertGasRecord([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log, [Sql("dbo.GasRecords", ConnectionStringSetting = "HomeControlSqlConnection")] IAsyncCollector<GasRecordDao> gasRecords)
        {
            try
            {
                log.LogInformation("Insert gas record.");

                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var item = JsonConvert.DeserializeObject<GasRecordDao>(body);

                await gasRecords.AddAsync(item);
                await gasRecords.FlushAsync();
            }
            catch (Exception e)
            {
                log.LogError(e, "Error while inserting gas records.");
                throw;
            }

            log.LogInformation("Gas record inserted.");

            return new OkResult();
        }

        [FunctionName("GetGasRecords")]
        public async Task<IActionResult> GetGasRecords([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log, [Sql("SELECT * FROM dbo.GasRecords ", CommandType = CommandType.Text, ConnectionStringSetting = "HomeControlSqlConnection")] IEnumerable<GasRecordDao> gasRecords)
        {
            log.LogInformation("Get gas records.");

            return await Task.FromResult(new OkObjectResult(gasRecords));
        }
    }
}
