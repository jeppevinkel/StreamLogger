using System.Collections.Generic;
using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace OpenVRNotificationPipeIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;

        [Description("Enable to prevent messages from the broadcaster from showing up.")]
        public bool IgnoreBroadcaster { get; set; } = false;

        [Description("Messages starting with any of the prefixes listed here will get ignored.")]
        public string[] IgnorePrefix { get; set; } = new[] {"!"};

        [Description("Experimental. Creates new folders for new games that allows a per game basis layout.")]
        public bool GameSpecificStyles { get; set; } = false;

        public string PipeHost { get; set; } = "ws://localhost";
        public int PipePort { get; set; } = 8077;
        public EventToggles EnabledEvents { get; set; } = new EventToggles();

        [Description("Enable to append the channel to the display name.")]
        public bool AppendChannel { get; set; } = false;

        public bool ShowDebugOutline { get; set; } = false;
    }

    public class EventToggles
    {
        public bool MessageEvent { get; set; } = false;
        public bool MessageWithBitsEvent { get; set; } = false;
        public bool FollowEvent { get; set; } = false;
        public bool NewSubscriptionEvent { get; set; } = false;
        public bool ReSubscriptionEvent { get; set; } = false;
        // public bool HostEvent { get; set; } = false;
    }
}