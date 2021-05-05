using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OpenVRNotificationPipeIntegration.EventHandlers;
using StreamLogger;
using Websocket.Client;

namespace OpenVRNotificationPipeIntegration
{
    public class PipeManager
    {
        public readonly WebsocketClient Client;
        
        public PipeManager(Main main)
        {
            Client = new WebsocketClient(new Uri($"{main.Config.PipeHost}:{main.Config.PipePort}"));
            Client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    Log.Info("[Pipe] Connected.");
                    return;
                }
                Log.Warn($"[Pipe] Reconnection happened, type: {info.Type}");
            });
            
            Client.DisconnectionHappened.Subscribe(info =>
            {
            Log.Warn($"[Pipe] Disconnected, type: {info.Type}\n{info.CloseStatusDescription}");
            });
            
            Client.ReconnectTimeout = null;
            
            Client.MessageReceived.Subscribe(msg => Log.Info($"[Pipe] Message received: {msg}"));
            
            Client.Start();
        }

        private static string FormatMessage(string imageData, PipeStyle style)
        {
            string msg = JsonSerializer.Serialize(new PipeMessage(imageData, style));
            Log.Debug($"[Pipe] Formatted msg: {msg}");
            return msg;
        }

        public void SendImage(Image img, PipeStyle style)
        {
            SendRaw(FormatMessage(img.ToBase64String(ImageFormat.Png),
                style));
        }

        public void SendRaw(string message)
        {
            if (!Client.IsRunning)
            {
                Log.Warn("[Pipe] Attempted to send message, but the pipe is not connected.");
                return;
            }

            Task.Run(() => Client.Send(message));
        }
        
        public class PipeMessage
        {
            [JsonPropertyName("custom")]
            public bool Custom { get; set; } = true;
            [JsonPropertyName("image")]
            public string ImageData { get; set; } = "";
            [JsonPropertyName("properties")]
            public PipeStyle.Properties Properties { get; set; } = new PipeStyle.Properties();
            [JsonPropertyName("transition")]
            public PipeStyle.Transition Transition { get; set; } = new PipeStyle.Transition();
            [JsonPropertyName("transition2")]
            public PipeStyle.Transition Transition2 { get; set; } = new PipeStyle.Transition();

            public PipeMessage(string imageData, PipeStyle pipeStyle)
            {
                ImageData = imageData;
                Properties = pipeStyle.MyProperties;
                Transition = pipeStyle.TransitionIn;
                Transition2 = pipeStyle.TransitionOut;
            }
        }
    }
}