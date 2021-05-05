using System;

namespace TwitchImplementation.TwitchBot.Auth
{
    public class TokenData
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public long Expiry { get; set; } = 0;
        public string[] Scope { get; set; } = Array.Empty<string>();
    }
}