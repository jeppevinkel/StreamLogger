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

            EventManager.ChatMessageEvent += ChatMessageEvent;
            EventManager.ChatMessageWithRewardEvent += ChatMessageWithRewardEvent;
            EventManager.HostNotificationEvent += HostNotificationEvent;
            EventManager.HostingStartedEvent += HostingStartedEvent;
            EventManager.HostingStoppedEvent += HostingStoppedEvent;
            EventManager.NewSubscriptionEvent += NewSubscriptionEvent;
            EventManager.ReSubscriptionEvent += ReSubscriptionEvent;
        }

        private void ChatMessageEvent(ChatMessageEventArgs e)
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

        private void ChatMessageWithRewardEvent(ChatMessageWithRewardEventArgs e)
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

            msg =
                $"{meta} {badges}{e.Message.DisplayName.Pastel(e.Message.Color)}: {e.Message.MessageContent} ({e.Message.RewardId})";

            Log.Info(msg);
        }

        public void HostNotificationEvent(HostNotificationEventArgs e)
        {
            var meta = $"[{e.HostNotification.TargetChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostNotification.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            if (e.HostNotification.IsAutoHost)
            {
                msg =
                    $"{meta} {e.HostNotification.HostingChannel} is hosting with auto host.";
            }
            else
            {
                msg =
                    $"{meta} {e.HostNotification.HostingChannel} is hosting with {e.HostNotification.Viewers} viewers.";
            }

            Log.Info(msg);
        }

        private void HostingStartedEvent(HostingStartedEventArgs e)
        {
            var meta = $"[{e.HostingStarted.HostingChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostingStarted.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            msg =
                $"{meta} Now hosting {e.HostingStarted.TargetChannel} with {e.HostingStarted.Viewers} viewers.";

            Log.Info(msg);
        }

        private void HostingStoppedEvent(HostingStoppedEventArgs e)
        {
            var meta = $"[{e.HostingStopped.HostingChannel}][{DateTimeOffset.FromUnixTimeSeconds(e.HostingStopped.Timestamp).ToLocalTime():HH:mm}]";

            string msg;

            msg =
                $"{meta} No longer hosting with {e.HostingStopped.Viewers} viewers.";

            Log.Info(msg);
        }

        private void ReSubscriptionEvent(ReSubscriptionEventArgs e)
        {
            var meta = $"[{e.Subscription.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Subscription.Timestamp).ToLocalTime():HH:mm}]";
        
            string modBadge = e.Subscription.Mod ? "[MOD]" : "";
            string subscriberBadge = e.Subscription.Subscriber ? "[SUB]" : "";

            var badges = $"{subscriberBadge}{modBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }

            string msg;

            msg =
                $"{meta} {e.Subscription.SystemMessage} with the following message: {e.Subscription.MessageContent}";

            Log.Info(msg);
        }

        private void NewSubscriptionEvent(NewSubscriptionEventArgs e)
        {
            var meta = $"[{e.Subscription.Channel}][{DateTimeOffset.FromUnixTimeSeconds(e.Subscription.Timestamp).ToLocalTime():HH:mm}]";
        
            string modBadge = e.Subscription.Mod ? "[MOD]" : "";
            string subscriberBadge = e.Subscription.Subscriber ? "[SUB]" : "";

            var badges = $"{subscriberBadge}{modBadge}";
            if (badges.Length != 0)
            {
                badges += " ";
            }

            string msg;

            msg =
                $"{meta} {e.Subscription.SystemMessage} with the following message: {e.Subscription.MessageContent}";

            Log.Info(msg);
        }
    }
}