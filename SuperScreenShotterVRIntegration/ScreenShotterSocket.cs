using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StreamLogger;
using StreamLogger.Api.MessageTypes;
using Websocket.Client;

namespace SuperScreenShotterVRIntegration
{
    public class ScreenShotterSocket
    {
        public readonly WebsocketClient Client;

        private uint _screenshotCount = 0;
        private Dictionary<uint, ChatMessageWithReward> _screenshotRequests = new ();

        public ScreenShotterSocket(Main main)
        {
            Client = new WebsocketClient(new Uri($"{main.Config.Host}:{main.Config.Port}"));
            Client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    Log.Info("[SSSVR] Connected.");
                    return;
                }

                Log.Warn($"[SSSVR] Reconnection happened, type: {info.Type}");
            });
            
            Client.DisconnectionHappened.Subscribe(info =>
            {
                Log.Warn($"[SSSVR] Disconnected, type: {info.Type}\n{info.CloseStatusDescription}");
            });

            Client.MessageReceived.Subscribe(msg =>
            {
                var res = JsonSerializer.Deserialize<ScreenshotResponse>(msg.Text);
                Log.Debug($"[SSSVR] Received screenshot: {res?.Nonce}");

                try
                {
                    if (res is null ||
                        !_screenshotRequests.TryGetValue(uint.Parse(res.Nonce), out ChatMessageWithReward message)) return;
                    
                    List<DiscordManager.Embed> embeds = new();
                    
                    embeds.Add(new DiscordManager.Embed
                    {
                        Color = int.Parse(message.Color.Trim('#'), NumberStyles.HexNumber),
                        Description = main.Config.ImageDescription.Replace("{message}", message.MessageContent),
                        Image = new DiscordManager.EmbedImage{Url = "attachment://image.png"},
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).ToString("s"),
                        Footer = main.CurrentApp is not null ? new DiscordManager.EmbedFooter{Text = main.CurrentApp} : null,
                    });
                    
                    DiscordManager.Send(main, message.DisplayName, message.AvatarUrl, embeds, Convert.FromBase64String(res.Image));
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
            
            Client.ReconnectTimeout = null;
            
            Client.Start();
        }

        public void TakeScreenshot(ChatMessageWithReward reward, int delay)
        {
            _screenshotCount++;
            var req = new ScreenshotRequest {Tag = reward.Username, Nonce = _screenshotCount.ToString()};
            string msg = JsonSerializer.Serialize(req);
            Log.Debug($"[SSSVR] Formatted screenshot request: {msg}");
            _screenshotRequests[_screenshotCount] = reward;
            SendRaw(msg);
        }

        public void SendRaw(string message)
        {
            if (!Client.IsRunning)
            {
                Log.Warn("[SSSVR] Attempted to send message, but the socket is not connected.");
                return;
            }
            
            Task.Run(() => Client.Send(message));
        }
        
        public class ScreenshotRequest
        {
            [JsonPropertyName("nonce")] public string Nonce { get; set; }

            [JsonPropertyName("tag")] public string Tag { get; set; }

            [JsonPropertyName("delay")] public int Delay { get; set; } = 2;
        }
        
        public class ScreenshotResponse
        {
            [JsonPropertyName("nonce")] public string Nonce { get; set; }
            [JsonPropertyName("image")] public string Image { get; set; }
        }
    }
}