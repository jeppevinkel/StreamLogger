using System.Collections.Generic;
using StreamLogger.Api.MessageTypes.MiscData;

namespace StreamLogger.Api.MessageTypes
{
    public class Subscription
    {
        public Dictionary<string, int> Badges;
        public string Color;
        public string DisplayName;
        public List<Emote> Emotes;
        public HashSet<string> Flags;
        public bool Mod;
        public bool Subscriber;
        /// <summary>
        /// Unix timestamp in seconds using UTC.
        /// </summary>
        public long Timestamp;
        public string UserType;
        public string Username;
        public string UserId;
        public string Channel;
        public string AvatarUrl;
        public string MessageContent;
        public SubscriptionPlan SubscriptionPlan;
        public string SubscriptionPlanName;
        public int CumulativeMonths;
        public bool ShouldShareStreak;
        public int StreakMonths;
        public string SystemMessage;
        
        public Subscription(Dictionary<string, int> badges, string color, string displayName, List<Emote> emotes, HashSet<string> flags, bool mod, bool subscriber, long timestamp, string userType, string username, string userId, string channel, string avatarUrl, string messageContent, SubscriptionPlan subscriptionPlan, string subscriptionPlanName, int cumulativeMonths, bool shouldShareStreak, int streakMonths, string systemMessage)
        {
            Badges = badges ?? new Dictionary<string, int>();
            Color = color;
            DisplayName = displayName;
            Emotes = emotes ?? new List<Emote>();
            Flags = flags;
            Mod = mod;
            Subscriber = subscriber;
            Timestamp = timestamp;
            UserType = username;
            Username = username;
            UserId = userId;
            Channel = channel;
            AvatarUrl = avatarUrl;
            MessageContent = messageContent;
            SubscriptionPlan = subscriptionPlan;
            SubscriptionPlanName = subscriptionPlanName;
            CumulativeMonths = cumulativeMonths;
            ShouldShareStreak = shouldShareStreak;
            StreakMonths = streakMonths;
            SystemMessage = systemMessage;
        }
    }
}