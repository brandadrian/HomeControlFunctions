using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HomeControlFunctions.Functions
{
    public class TestFunctions
    {
        private readonly IConfiguration _configuration;

        public TestFunctions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("Test")]
        public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Request received.");

            var value = _configuration.GetSection("TestValue").Value;

            return await Task.FromResult(new OkObjectResult($"Function works... {value}"));
        }
    }
}
