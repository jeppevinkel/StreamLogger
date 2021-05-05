using StreamLogger.Api.Interfaces;

namespace ConsoleIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;
    }
}