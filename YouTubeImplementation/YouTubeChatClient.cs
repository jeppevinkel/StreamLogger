using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using StreamLogger;
using YouTubeImplementation.EventHandlers;

namespace YouTubeImplementation
{
    public class YouTubeChatClient
    {
        private Thread _chatThread;
        private bool _disconnect;
        private string _liveChatId;
        private YouTubeService _service;
        private Dictionary<string, string> _userDictionary;
        private DateTimeOffset _lastRequest = DateTimeOffset.Now;
        
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | 
                             JsonNumberHandling.WriteAsString
        };

        private int x = 0;

        public bool IsConnected { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;
        public string ChannelName { get; private set; }

        public void PrintRequest(DateTimeOffset lastRequest)
        {
            Log.Info($"Last Request: {(DateTimeOffset.Now - lastRequest).TotalMilliseconds}ms ago.");
            lastRequest = DateTimeOffset.Now;
        }

        public async Task InitializeAsync(string secret, string id)
        {
            _userDictionary = ReadUserData();
            
            var secrets = new ClientSecrets {ClientId = id, ClientSecret = secret};
            PrintRequest(_lastRequest);
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { YouTubeService.Scope.YoutubeReadonly },
                "user", CancellationToken.None, new FileDataStore("YouTube.StreamLogger"));

            Log.Info(x++);
            _service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer =  credential,
                ApplicationName = "StreamLogger"
            });
            
            Log.Info(x++);
            await Task.Delay(100);
            PrintRequest(_lastRequest);
            ChannelListResponse channels = await _service.Channels.List("snippet").Configure(request =>
                {
                    request.Mine = true;
                })
                .ExecuteAsync();
            Log.Info(x++);
            if (channels.Items.Count > 0)
            {
                ChannelName = channels.Items[0].Snippet.Title;
            }
            Log.Info(x++);
            await Task.Delay(100);
            _liveChatId = await GetLiveChatIdAsync();
        }
        
        public async Task ConnectAsync()
        {
            if (!IsInitialized)
            {
                Log.Info("[YouTube] Initializing...");
                await InitializeAsync(Main.Instance.Config.ClientSecret, Main.Instance.Config.ClientId);
                Log.Info("[YouTube] Initialized.");
            }

            if (!string.IsNullOrEmpty(_liveChatId))
            {
                _chatThread = new Thread(async () =>
                {
                    Log.Info("[YouTube] Thread started...");
                    var first = true;
                    var pageToken = "";
                    var pollingInterval = 0L;
                    var retryAttempts = 0;

                    while (!_disconnect)
                    {
                        try
                        {
                            LiveChatMessagesResource.ListRequest request =
                                _service.LiveChatMessages.List(_liveChatId, "snippet,authorDetails");
                            request.PageToken = string.IsNullOrEmpty(pageToken) ? "" : pageToken;
                            PrintRequest(_lastRequest);
                            LiveChatMessageListResponse messages = await request.ExecuteAsync();

                            if (messages is null) continue;
                            pageToken = messages.NextPageToken;
                            Log.Debug($"New Polling Interval: {messages.PollingIntervalMillis}");
                            pollingInterval = messages.PollingIntervalMillis ?? 1000;

                            if (!first)
                            {
                                foreach (LiveChatMessage message in messages.Items)
                                {
                                    PushUserData(message.AuthorDetails.ChannelId, message.AuthorDetails.DisplayName);
                                    ChatEventHandler.OnChatMessage(message);
                                    int pos = -1;
                                    while (0 >= (pos = message.Snippet.DisplayMessage.IndexOf('@', pos + 1)))
                                    {
                                        
                                    }
                                }
                            }

                            retryAttempts = 0;
                            first = false;
                            await Task.Delay(((int) pollingInterval)*10);
                        }
                        catch (Exception e)
                        {
                            if (retryAttempts >= 5)
                            {
                                Log.Error($"[YouTube] {e}");
                            }

                            Log.Warn($"[YouTube] {e}");
                            retryAttempts++;
                            await Task.Delay(retryAttempts * 1000);
                        }
                    }
                });
                
                _chatThread.Start();
                IsConnected = true;
            }
            else
            {
                Log.Info("[YouTube] No live chat detected.");
            }
        }

        public async Task Disconnect()
        {
            _disconnect = true;
            while (_chatThread.IsAlive)
            {
                await Task.Delay(100);
            }

            IsConnected = false;
        }

        private async Task<string> GetLiveChatIdAsync()
        {
            LiveBroadcastListResponse broadcasts;
            try
            {
                PrintRequest(_lastRequest);
                broadcasts = await _service.LiveBroadcasts.List("snippet").Configure(request => request.Mine = true).ExecuteAsync();
            }
            catch (Exception e)
            {
                Log.Error($"[YouTube] {e}");
                return null;
            }
            
            Log.Info($"[YouTube] Active stream: {broadcasts?.Items?.FirstOrDefault()?.Snippet.Title}");
            return broadcasts?.Items?.FirstOrDefault()?.Snippet?.LiveChatId;
        }

        public void PushUserData(string id, string name)
        {
            if (_userDictionary.ContainsKey(id) && _userDictionary[id] != name)
            {
                _userDictionary[id] = name;
                SaveUserData(_userDictionary);
            }
            else if (!_userDictionary.ContainsKey(id))
            {
                _userDictionary.Add(id, name);
                SaveUserData(_userDictionary);
            }
        }

        public static void SaveUserData(Dictionary<string,string> data)
        {
            Directory.CreateDirectory(Main.Instance.BaseDir);
            
            string filePath = Path.Combine(Main.Instance.BaseDir, "userDic.json");
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }
        
        public static Dictionary<string, string> ReadUserData()
        {
            Dictionary<string, string> userData;
            string filePath = Path.Combine(Main.Instance.BaseDir, "userDic.json");
            
            if (!File.Exists(filePath))
            {
                userData = new ();
                return userData;
            }
            
            string rawData = File.ReadAllText(filePath);

            try
            {
                userData = JsonSerializer.Deserialize<Dictionary<string,string>>(rawData, options);
            }
            catch
            {
                userData = new();
            }
            
            return userData;
        }
    }
}