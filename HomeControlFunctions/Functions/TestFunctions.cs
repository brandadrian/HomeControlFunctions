using HomeControlFunctions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HomeControlFunctions.Functions
{
    public class TestFunctions
    {
        private readonly IOptions<ConfigurationOptions> _configurationOptions;

        public TestFunctions(IOptions<ConfigurationOptions> configurationOptions)
        {
            _configurationOptions = configurationOptions;
        }

        [FunctionName("Test")]
        public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Request received.");

            var value = _configurationOptions.Value.TestValue;

            return await Task.FromResult(new OkObjectResult($"Function works... {value}"));
        }
    }
}
