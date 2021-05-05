using System;
using System.Collections.Specialized;
using System.Net;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;

namespace DiscordIntegration
{
    public class DiscordIntegration : Integration<Config>
    {
        public override void Init()
        {
            base.Init();

            if (Config.Events.ChatMessage)
                EventManager.ChatMessageEvent += ChatMessageEvent;
            if (Config.Events.ChatMessage)
                EventManager.ChatMessageWithRewardEvent += ChatMessageWithRewardEvent;
            if (Config.Events.HostNotification)
                EventManager.HostNotificationEvent += HostNotificationEvent;
            if (Config.Events.HostingStarted)
                EventManager.HostingStartedEvent += HostingStartedEvent;
            if (Config.Events.HostingStopped)
                EventManager.HostingStoppedEvent += HostingStoppedEvent;
        }

        private void ChatMessageEvent(ChatMessageEventArgs e)
        {
            var meta = $"[#{e.Message.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Message.Timestamp).ToLocalTime():HH:mm}]";

            string modBadge = e.Message.Mod ? Config.Badges.ModeratorBadge : "";
            string broadcasterBadge = e.Message.Broadcaster ? Config.Badges.BroadcasterBadge : "";
            string subscriberBadge = e.Message.Subscriber ? Config.Badges.SubscriberBadge : "";
            
            var badges = $"{subscriberBadge}{modBadge}{broadcasterBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }

            string msg;
            
            if (e.Message.Flags.Contains("IsMe"))
            {
                msg =
                    $"{meta} {badges}***{e.Message.DisplayName}*** *{e.Message.MessageContent}*";
            }
            else
            {
                msg =
                    $"{meta} {badges}{e.Message.DisplayName}: {e.Message.MessageContent}";
            }

            SendDiscordWebhook(msg);
        }

        private void ChatMessageWithRewardEvent(ChatMessageWithRewardEventArgs e)
        {
            var meta = $"[#{e.Message.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Message.Timestamp).ToLocalTime():HH:mm}]";

            string modBadge = e.Message.Mod ? Config.Badges.ModeratorBadge : "";
            string broadcasterBadge = e.Message.Broadcaster ? Config.Badges.BroadcasterBadge : "";
            string subscriberBadge = e.Message.Subscriber ? Config.Badges.SubscriberBadge : "";
            
            var badges = $"{subscriberBadge}{modBadge}{broadcasterBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }

            string msg;

            msg =
                $"{meta} {badges}{e.Message.DisplayName}: {e.Message.MessageContent} ({e.Message.RewardId})";

            SendDiscordWebhook(msg);
        }

        public void HostNotificationEvent(HostNotificationEventArgs e)
        {
            var meta = $"[{e.HostNotification.TargetChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostNotification.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            if (e.HostNotification.IsAutoHost)
            {
                msg =
                    $"{meta} *{e.HostNotification.HostingChannel} is hosting with auto host.*";
            }
            else
            {
                msg =
                    $"{meta} **{e.HostNotification.HostingChannel} is hosting with {e.HostNotification.Viewers} viewers.**";
            }

            SendDiscordWebhook(msg);
        }

        private void HostingStartedEvent(HostingStartedEventArgs e)
        {
            var meta = $"[{e.HostingStarted.HostingChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostingStarted.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            msg =
                $"{meta} **Now hosting {e.HostingStarted.TargetChannel} with {e.HostingStarted.Viewers} viewers.**";

            SendDiscordWebhook(msg);
        }

        private void HostingStoppedEvent(HostingStoppedEventArgs e)
        {
            var meta = $"[{e.HostingStopped.HostingChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostingStopped.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            msg =
                $"{meta} *No longer hosting with {e.HostingStopped.Viewers} viewers.*";

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