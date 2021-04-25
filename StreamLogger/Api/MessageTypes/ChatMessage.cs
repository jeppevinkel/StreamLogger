using System.Collections.Generic;
using StreamLogger.Api.MessageTypes.MiscData;

namespace StreamLogger.Api.MessageTypes
{
    public struct ChatMessage
    {
        public Dictionary<string, int> Badges;
        public string Color;
        public string DisplayName;
        public List<Emote> Emotes;
        public string Flags;
        public bool Mod;
        public bool Subscriber;
        public bool Broadcaster;
        /// <summary>
        /// Unix timestamp in seconds using UTC.
        /// </summary>
        public long Timestamp;
        public string UserType;
        public string Username;
        public string Channel;
        public string MessageContent;

        public ChatMessage(Dictionary<string, int> badges, string color, string displayName, List<Emote> emotes, string flags, bool mod, bool subscriber, bool broadcaster, long timestamp, string userType, string username, string channel, string messageContent)
        {
            Badges = badges ?? new Dictionary<string, int>();
            Color = color;
            DisplayName = displayName;
            Emotes = emotes ?? new List<Emote>();
            Flags = flags;
            Mod = mod;
            Subscriber = subscriber;
            Broadcaster = broadcaster;
            Timestamp = timestamp;
            UserType = username;
            Username = username;
            Channel = channel;
            MessageContent = messageContent;
        }
    }
}