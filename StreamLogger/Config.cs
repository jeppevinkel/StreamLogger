using System.Collections.Generic;
using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace StreamLogger
{
    public class Config
    {
        [Description("Test config value!")] public string TestConfig = "Default value";
        [Description("Test config2 value!")] public string TestConfig2 = "Default value2";

        [Description("Integration specific configurations.")]
        public Dictionary<string, IConfig> Integrations = new Dictionary<string, IConfig>();
        
        
        [Description("Test config3 value!")] public string TestConfig3 = "Default value3";
    }
}