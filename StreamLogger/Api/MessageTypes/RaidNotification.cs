using System.Collections.Generic;
using StreamLogger.Api.MessageTypes.MiscData;

namespace StreamLogger.Api.MessageTypes
{
    public class RaidNotification
    {
        public string TargetChannel;
        public string RaidingChannel;
        
        public Dictionary<string, int> Badges;
        public string Color;
        public string DisplayName;
        // public List<Emote> Emotes;
        public bool Mod;
        public bool Subscriber;
        public long Timestamp;
        public string UserType;
        public string Username;
        public string UserId;
        public string AvatarUrl;
        public string SystemMessage;

        public RaidNotification(string targetChannel, string raidingChannel, Dictionary<string, int> badges,
            string color, string displayName, bool mod, bool subscriber, long timestamp,
            string userType, string username, string userId, string avatarUrl, string systemMessage)
        {
            TargetChannel = targetChannel;
            RaidingChannel = raidingChannel;
            Badges = badges ?? new Dictionary<string, int>();
            Color = color;
            DisplayName = displayName;
            // Emotes = emotes ?? new List<Emote>();
            Mod = mod;
            Subscriber = subscriber;
            Timestamp = timestamp;
            UserType = userType;
            Username = username;
            UserId = userId;
            AvatarUrl = avatarUrl;
            SystemMessage = systemMessage;
        }

        public RaidNotification()
        {
        }
    }
}