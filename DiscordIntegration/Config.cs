using System.Collections.Generic;
using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace DiscordIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = true;
        
        [Description("Webhook used for Discord messages.")]
        public string Webhook { get; set; } = "https://discord.com/api/webhooks/774817906130944020/WiN2UesxYFb4T1htrbR_fE6-8323Rre1WR8Wn1fSJLTL526TZXAyksb0z0JH07fqItoI";
        
        [Description("The interval at which messages are pushed to Discord.")]
        public int LogInterval { get; set; } = 1000;

        [Description("Name of the Discord chat bot.")] public string BotName { get; set; } = "Stream Log";

        public BadgeSymbols Badges { get; set; } = new BadgeSymbols();
        
        public class BadgeSymbols
        {
            public string ModeratorBadge { get; set; } = "[🛡️]";
            public string BroadcasterBadge { get; set; } = "[📣]";
            public string SubscriberBadge { get; set; } = "[💸]";
        }
    }
}