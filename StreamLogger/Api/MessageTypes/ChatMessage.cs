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
        public HashSet<string> Flags;
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
        public int Bits;
        public string AvatarUrl;
        public string MessageContent;

        public ChatMessage(Dictionary<string, int> badges, string color, string displayName, List<Emote> emotes, HashSet<string> flags, bool mod, bool subscriber, bool broadcaster, long timestamp, string userType, string username, string channel, int bits, string avatarUrl, string messageContent)
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
            UserType = userType;
            Username = username;
            Channel = channel;
            Bits = bits;
            AvatarUrl = avatarUrl;
            MessageContent = messageContent;
        }
    }
}