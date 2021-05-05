using System;

namespace StreamLogger.Api.MessageTypes
{
    public class Reward
    {
        public string Username;
        public string DisplayName;
        public string Channel;
        public string ChannelId;
        public Guid RewardId;
        public string RewardTitle;
        public string RewardPrompt;
        public int RewardCost;
        /// <summary>
        /// Unix timestamp in seconds using UTC.
        /// </summary>
        public long Timestamp;
    }
}