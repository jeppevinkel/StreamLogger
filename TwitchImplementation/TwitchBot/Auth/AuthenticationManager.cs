using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamLogger;
using StreamLogger.Api;

namespace TwitchImplementation.TwitchBot.Auth
{
    public class AuthenticationManager
    {
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | 
                             JsonNumberHandling.WriteAsString
        };

        public static TokenData TokenData = new TokenData();

        private static Config Config => Main.Instance.Config;
        
        static readonly string DataDir = Path.Combine(Paths.Implementations, Main.Instance.Name);
        static readonly string DataPath = Path.Combine(DataDir, "TokenData.json");

        public static bool Authenticate()
        {
            TokenData = ReadTokenData();

            if (string.IsNullOrWhiteSpace(Config.AccessToken) && string.IsNullOrWhiteSpace(Config.RefreshToken) && !string.IsNullOrWhiteSpace(TokenData.AccessToken) && !string.IsNullOrWhiteSpace(TokenData.RefreshToken))
            {
                Config.RefreshToken = TokenData.RefreshToken;
                RefreshToken();
            }
            
            if (string.IsNullOrWhiteSpace(Config.RefreshToken) || string.IsNullOrWhiteSpace(Config.ClientId) || string.IsNullOrWhiteSpace(Config.ClientSecret))
            {
                Log.Warn($"[Twitch] The bot requires the ClientId, ClientSecret, and RefreshToken to be filled out in the config to authenticate with Twitch.");
                return false;
            }

            DateTimeOffset expiry = DateTimeOffset.FromUnixTimeSeconds(Config.Expiry);
            bool expired = expiry.CompareTo(DateTimeOffset.UtcNow) <= 0;
            
            Log.Debug($"[Twitch] The token is going to expire on {expiry.ToLocalTime():G}");

            if (!expired) return ValidateTokenData();
            Log.Warn("[Twitch] The access token was expired, so I'm requesting a refresh from Twitch.");
            try
            {
                RefreshToken();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return ValidateTokenData();
        }
        
        public static void RefreshToken()
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", Config.RefreshToken },
                { "client_id", Config.ClientId },
                { "client_secret", Config.ClientSecret }
            };

            var content = new FormUrlEncodedContent(values);

            var response = Main.client.PostAsync("https://id.twitch.tv/oauth2/token", content).Result;

            var responseString = response.Content.ReadAsStringAsync().Result;

            var responseData = JsonSerializer.Deserialize<RefreshResponse>(responseString, options);
            if (responseData is not null)
            {
                DateTimeOffset tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(responseData.ExpiresIn);

                TokenData.Expiry = tokenExpiry.ToUnixTimeSeconds();
                TokenData.Scope = responseData.Scope;
                TokenData.AccessToken = responseData.AccessToken;
                TokenData.RefreshToken = responseData.RefreshToken;
                Config.Expiry = tokenExpiry.ToUnixTimeSeconds();
                Config.Scope = responseData.Scope;
                Config.AccessToken = responseData.AccessToken;
                Config.RefreshToken = responseData.RefreshToken;

                Config.Save();
            }
        }

        public static bool ValidateTokenData()
        {
            Main.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", Config.AccessToken);
            var response = Main.client.GetAsync("https://id.twitch.tv/oauth2/validate").Result;

            var responseString = response.Content.ReadAsStringAsync().Result;
            
            return !responseString.Contains("\"status\":401");
        }

        public static TokenData ReadTokenData()
        {
            TokenData tokenData;
            
            if (!File.Exists(DataPath))
            {
                tokenData = new TokenData();
                return tokenData;
            }
            
            string rawData = File.ReadAllText(DataPath);

            try
            {
                tokenData = JsonSerializer.Deserialize<TokenData>(rawData, options);
            }
            catch
            {
                tokenData = new TokenData();
            }
            
            return tokenData;
        }
    }
}