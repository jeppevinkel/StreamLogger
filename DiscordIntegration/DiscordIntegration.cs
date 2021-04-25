using System;
using System.Collections.Specialized;
using System.Net;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;

namespace DiscordIntegration
{
    public class DiscordIntegration : Integration<Config>
    {
        public override void Init()
        {
            base.Init();
        
            EventManager.RaiseChatMessageEvent += ChatMessageEvent;
        }

        private void ChatMessageEvent(object sender, ChatMessageEventArgs e)
        {
            var meta = $"[{e.Message.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Message.Timestamp).ToLocalTime():HH:mm}]";

            string modBadge = e.Message.Mod ? Config.Badges.ModeratorBadge : "";
            string broadcasterBadge = e.Message.Broadcaster ? Config.Badges.BroadcasterBadge : "";
            string subscriberBadge = e.Message.Subscriber ? Config.Badges.SubscriberBadge : "";
            
            var badges = $"{subscriberBadge}{modBadge}{broadcasterBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }
            
            var msg =
                $"{meta} {badges} {e.Message.DisplayName}: {e.Message.MessageContent}";
            
            SendDiscordWebhook(msg);
        }

        private void SendDiscordWebhook(string message)
        {
            var discordParams = new NameValueCollection
            {
                {"username", Config.BotName},
                {"content", message}
            };

            new WebClient().UploadValues(Config.Webhook, discordParams);
        }
    }
}