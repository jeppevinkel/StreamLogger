using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using Websocket.Client;

namespace OpenVR2WSImplementation
{
    public class Main : Implementation<Config>
    {
        public static WebsocketClient Client;
        public static readonly string AppIdExpression = "(?<={\\\"key\\\":\\\"ApplicationInfo\\\",\\\"data\\\":{\\\"id\\\":\\\"steam\\.app\\.)(.*?)(?=\\\",)";
        public static readonly string CloseAppExpression = "{\\\"key\\\":\\\"ApplicationInfo\\\",\\\"data\\\":{\\\"id\\\":\\\"\\\"";
        public static readonly string AppInfoUrl = "https://store.steampowered.com/api/appdetails/?appids=";

        public static int CurrentGame = 0;

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();
            
            Client = new WebsocketClient(new Uri($"{Config.Host}:{Config.Port}"));
            Client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    Log.Info("[OVR2WS] Connected.");
                    return;
                }
                Log.Warn($"[OVR2WS] Reconnection happened, type: {info.Type}");
            });
            
            Client.DisconnectionHappened.Subscribe(info =>
            {
                Log.Warn($"[OVR2WS] Disconnected, type: {info.Type}\n{info.CloseStatusDescription}");
            });
            
            Client.ReconnectTimeout = null;
            
            Client.MessageReceived.Subscribe(HandleMessage);
            
            Client.Start();
        }

        public static void HandleMessage(ResponseMessage msg)
        {
            Match matchGame = Regex.Match(msg.Text, AppIdExpression);
            Match matchCloseGame = Regex.Match(msg.Text, CloseAppExpression);
            
            
            try
            {
                if (matchGame.Success)
                {
                    if (!int.TryParse(matchGame.Value, out int appId)) return;

                    if (appId != CurrentGame)
                    {
                        CurrentGame = appId;

                        using var client = new WebClient();
                        string data = client.DownloadString($"{AppInfoUrl}{appId}");

                        var appInfoRequests = JsonSerializer.Deserialize<Dictionary<string, AppInfoRequest>>(data);

                        if (appInfoRequests == null) return;
                        foreach ((string key, AppInfoRequest value) in appInfoRequests)
                        {
                            if (!value.Success) continue;
                            try
                            {
                                var gameChange = new GameChange
                                {
                                    GameDescription = value.Data.ShortDescription,
                                    GameId = appId,
                                    GameName = value.Data.Name,
                                    GameCoverUrl = $"https://steamcdn-a.akamaihd.net/steam/apps/{appId}/library_600x900_2x.jpg",
                                    GameHeaderUrl = $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/header.jpg?t=1598910876",
                                    GameIsFree = value.Data.IsFree
                                };

                                var gameChangeEventArgs = new GameChangeEventArgs(gameChange);
                            
                                EventManager.OnGameChangeEvent(gameChangeEventArgs);
                            }
                            catch (Exception e)
                            {
                                Log.Error($"[OVR2WS] {e}");
                            }
                        }
                    }
                }
                else if (matchCloseGame.Success)
                {
                    if (CurrentGame == 0) return;
                    CurrentGame = 0;

                    var gameChange = new GameChange
                    {
                        GameDescription = null,
                        GameId = 0,
                        GameName = null,
                        GameCoverUrl = null,
                        GameHeaderUrl = null,
                        GameIsFree = true
                    };
                
                    EventManager.OnGameChangeEvent(new GameChangeEventArgs(gameChange));
                }
            
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public class AppInfoRequest
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("data")]
        public AppInfo Data { get; set; }
    }

    public class AppInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("is_free")]
        public bool IsFree { get; set; }
        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }
    }
}