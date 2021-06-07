using System.Collections.Generic;
using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace TwitchImplementation
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;

        public List<string> Channels { get; set; } = new List<string>();

        public string Username { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        
        public string AccessToken { get; set; }
        
        public string RefreshToken { get; set; }

        
        [Description("The scope will get filled automatically when authenticated.")]
        public string[] Scope { get; set; } = {};

        [Description("The expiry will get filled automatically when authenticated. You can set this to 0 to force a token refresh.")]
        public long Expiry { get; set; } = 0;
    }
}