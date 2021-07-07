using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;

namespace SuperScreenShotterVRIntegration
{
    public class Main : Integration<Config>
    {
        private static Main singleton = new Main();
        public static Main Instance => singleton;
        
        private ScreenShotterSocket _shotterSocket;
        private Dictionary<string, ScreenshotRequest> _queuedScreenshots = new();
        
        public string CurrentApp = String.Empty;

        public override void Init()
        {
            base.Init();

            _shotterSocket = new ScreenShotterSocket(this);
            EventManager.ChatMessageWithRewardEvent += OnChatMessageWithReward;
            EventManager.GameChangeEvent += OnGameChange;
            
            Broadcaster.Listen("sssvr_cb", e =>
            {
                Log.Debug($"({e.Nonce}) {e.Message}");
                
                foreach (string queuedScreenshotsKey in _queuedScreenshots.Keys)
                {
                    Log.Debug(queuedScreenshotsKey);
                }
                
                switch (e.Message)
                {
                    case "DONE" when _queuedScreenshots.ContainsKey(e.Nonce):
                    {
                        ScreenshotRequest screenshotRequest = _queuedScreenshots[e.Nonce];
                        _shotterSocket.TakeScreenshot(screenshotRequest.Message, Config.Delay);
                        // _queuedScreenshots.Remove(e.Nonce);
                        break;
                    }
                    case "OK" when _queuedScreenshots.ContainsKey(e.Nonce):
                        _queuedScreenshots[e.Nonce].WaitForTts = true;
                        break;
                }
                if (_queuedScreenshots.ContainsKey(e.Nonce)) Log.Debug($"({e.Nonce}) {_queuedScreenshots[e.Nonce].WaitForTts}");
            });
        }

        private void OnChatMessageWithReward(ChatMessageWithRewardEventArgs e)
        {
            if (e.Message.RewardId != Config.RewardId) return;

            string nonce = Broadcaster.GetNonce();
            
            var screenshotRequest = new ScreenshotRequest(e.Message);
            _queuedScreenshots.Add(nonce, screenshotRequest);
            Broadcaster.Publish("tts", JsonSerializer.Serialize(new TtsBroadcast(e.Message, Config)), nonce, "sssvr_cb");
            
            Log.Debug("Checking for tts response...");
            if (!_queuedScreenshots[nonce].WaitForTts)
            {
                Log.Debug("No tts response.");
                _shotterSocket.TakeScreenshot(screenshotRequest.Message, Config.Delay);
                _queuedScreenshots.Remove(nonce);
            }

            // Task.Delay(100).ContinueWith(o =>
            // {
            //     Log.Debug("Checking for tts response...");
            //     if (!_queuedScreenshots[nonce].WaitForTts)
            //     {
            //         Log.Debug("No tts response.");
            //         _shotterSocket.TakeScreenshot(screenshotRequest.Message, Config.Delay);
            //         _queuedScreenshots.Remove(nonce);
            //     }
            // });
        }
        
        private void OnGameChange(GameChangeEventArgs e)
        {
            CurrentApp = e.GameChange.GameName;
        }
        
        private class TtsBroadcast
        {
            [JsonPropertyName("userId")] public string UserId { get; set; }
            [JsonPropertyName("displayName")] public string DisplayName { get; set; }
            [JsonPropertyName("message")] public string Message { get; set; }

            public TtsBroadcast(ChatMessageWithReward message, Config cfg)
            {
                UserId = message.Username;
                DisplayName = message.DisplayName;
                Message = cfg.ImageDescription.Replace("{message}", message.MessageContent);
            }
        }
        
        private class ScreenshotRequest
        {
            public ChatMessageWithReward Message;
            public bool WaitForTts = false;

            public ScreenshotRequest(ChatMessageWithReward message)
            {
                Message = message;
            }
        }
    }
}