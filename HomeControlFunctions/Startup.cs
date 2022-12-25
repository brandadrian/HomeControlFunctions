using HomeControlFunctions.Configuration;
using HomeControlFunctions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(HomeControlFunctions.Startup))]
namespace HomeControlFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<OcrService>();
            builder.Services.AddOptions<ConfigurationOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Values").Bind(settings);
                });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .SetBasePath(context.ApplicationRootPath)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"settings.{context.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
    }
}
