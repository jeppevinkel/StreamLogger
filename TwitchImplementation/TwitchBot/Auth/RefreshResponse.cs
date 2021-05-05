using System.Text.Json.Serialization;

namespace TwitchImplementation.TwitchBot.Auth
{
    public class RefreshResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("scope")]
        public string[] Scope { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }
}