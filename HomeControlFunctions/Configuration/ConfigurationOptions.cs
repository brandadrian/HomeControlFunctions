using System.Collections.Generic;

namespace HomeControlFunctions.Configuration
{
    public class ConfigurationOptions
    {
        public string CognitiveServicesSubscriptionKey { get; set; }

        public string CognitiveServicesEndpoint { get; set; }

        public string TestValue { get; set; }
        
        public string HomeControlSqlConnection { get; set; }
        
        public string SabahudinTelegramApiKey { get; set; }

        public string SabahudinTelegramResponses { get; set; }
    }
}
