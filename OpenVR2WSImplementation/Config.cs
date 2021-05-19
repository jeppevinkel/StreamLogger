using StreamLogger.Api.Interfaces;

namespace OpenVR2WSImplementation
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; }

        public string Host { get; set; } = "ws://localhost";

        public int Port { get; set; } = 7708;
    }
}