using System;
using Pastel;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;

namespace ConsoleIntegration
{
    public class Integration : Integration<Config>
    {
        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            EventManager.RaiseChatMessageEvent += ChatMessageEvent;
        }
        
        private void ChatMessageEvent(object sender, ChatMessageEventArgs e)
        {
            var meta = $"[{e.Message.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Message.Timestamp).ToLocalTime():HH:mm}]";
        
            string modBadge = e.Message.Mod ? "[MOD]" : "";
            string broadcasterBadge = e.Message.Broadcaster ? "[BROADCASTER]" : "";
            string subscriberBadge = e.Message.Subscriber ? "[SUB]" : "";

            var badges = $"{subscriberBadge}{modBadge}{broadcasterBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }

            string msg;
            
            if (e.Message.Flags.Contains("IsMe"))
            {
                msg =
                    $"{meta} {badges}{e.Message.DisplayName.Pastel(e.Message.Color)} {e.Message.MessageContent.Pastel(e.Message.Color)}";
            }
            else
            {
                msg =
                    $"{meta} {badges}{e.Message.DisplayName.Pastel(e.Message.Color)}: {e.Message.MessageContent}";
            }

            Log.Info(msg);
        }
    }
}