using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StreamLogger;
using StreamLogger.Api;
using YamlDotNet.Core.Tokens;

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
        
        static readonly string DataDir = Path.Combine(Paths.Implementations, Main.Instance.Name);
        static readonly string DataPath = Path.Combine(DataDir, "TokenData.json");

        public static bool Authenticate()
        {
            TokenData = ReadTokenData();
            if (string.IsNullOrWhiteSpace(TokenData.RefreshToken) || string.IsNullOrWhiteSpace(Main.Instance.Config.ClientId) || string.IsNullOrWhiteSpace(Main.Instance.Config.ClientSecret))
            {
                Log.Warn($"The bot requires ClientId and ClientSecret to be filled out in the config, as well as the RefreshToken in {DataPath} to authenticate with Twitch.");
                return false;
            }

            DateTimeOffset expiry = DateTimeOffset.FromUnixTimeSeconds(TokenData.Expiry);
            bool expired = expiry.CompareTo(DateTimeOffset.UtcNow) <= 0;

            if (expired)
            {
                Log.Warn("The access token was expired, so I'm requesting a refresh from Twitch.");
                try
                {
                    RefreshToken();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return ValidateTokenData();
        }
        
        public static void RefreshToken()
        {
            var values = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", TokenData.RefreshToken },
                { "client_id", Main.Instance.Config.ClientId },
                { "client_secret", Main.Instance.Config.ClientSecret }
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
                
                SaveTokenData(TokenData);
            }
        }

        public static bool ValidateTokenData()
        {
            var response = Main.client.GetAsync("https://id.twitch.tv/oauth2/validate").Result;

            var responseString = response.Content.ReadAsStringAsync().Result;
            
            return !responseString.Contains("\"status\":401");
        }

        public static TokenData ReadTokenData()
        {
            TokenData tokenData;
            
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
            if (!File.Exists(DataPath))
            {
                tokenData = new TokenData();
                SaveTokenData(tokenData);
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
                SaveTokenData(tokenData);
                Log.Warn("TokenData is in invalid format. Returning empty object.");
            }

            Main.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", tokenData.AccessToken);
            return tokenData;
        }

        public static void SaveTokenData(TokenData tokenData)
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            string rawData = JsonSerializer.Serialize(tokenData);
            File.WriteAllText(DataPath, rawData);
        }
    }
}