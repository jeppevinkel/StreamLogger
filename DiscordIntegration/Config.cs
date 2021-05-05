using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace DiscordIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;
        
        [Description("Webhook used for Discord messages.")]
        public string Webhook { get; set; } = "https://discord.com/api/webhooks/xxxxxxxxxxx/xxxxxxxxxxxxx";
        
        [Description("The interval at which messages are pushed to Discord.")]
        public int LogInterval { get; set; } = 1000;

        [Description("Name of the Discord chat bot.")] public string BotName { get; set; } = "Stream Log";

        public BadgeSymbols Badges { get; set; } = new BadgeSymbols();

        [Description("Enable/disable individual events.")]
        public EnabledEvents Events { get; set; } = new EnabledEvents();
        
        public class BadgeSymbols
        {
            public string ModeratorBadge { get; set; } = "[🛡️]";
            public string BroadcasterBadge { get; set; } = "[📣]";
            public string SubscriberBadge { get; set; } = "[💸]";
        }
        
        public class EnabledEvents
        {
            [Description("Called when a message is sent in your channel.")]
            public bool ChatMessage { get; set; } = true;
            [Description("Called when you start hosting someone.")]
            public bool HostingStarted { get; set; } = true;
            [Description("Called when you stop hosting someone.")]
            public bool HostingStopped { get; set; } = true;
            [Description("Called when your channel is being hosted.")]
            public bool HostNotification { get; set; } = true;
        }
    }
}