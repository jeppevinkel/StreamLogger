using System;
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
                    
            var msg =
                $"{meta} {badges}{e.Message.DisplayName}: {e.Message.MessageContent}";
                    
            Log.Info(msg);
        }
    }
}