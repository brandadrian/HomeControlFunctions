using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HomeControlFunctions.Models;
using Newtonsoft.Json;

namespace HomeControlFunctions.Functions
{
    public class GasRecordFunctions
    {
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
        public async Task<IActionResult> GetGasRecords([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log, [Sql("SELECT * FROM dbo.GasRecords", CommandType = CommandType.Text, ConnectionStringSetting = "HomeControlSqlConnection")] IEnumerable<GasRecordDao> gasRecords)
        {
            log.LogInformation("Get gas records.");

            return await Task.FromResult(new OkObjectResult(gasRecords));
        }
    }
}
