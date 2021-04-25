using System.Collections.Generic;
using StreamLogger.Api.Interfaces;

namespace TwitchImplementation
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;

        public List<string> Channels { get; set; } = new List<string>();
    }
}