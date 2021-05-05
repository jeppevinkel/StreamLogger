namespace StreamLogger.Api.MessageTypes
{
    public struct Follow
    {
        public string Channel;
        public string ChannelId;
        public string DisplayName;
        /// <summary>
        /// Unix timestamp in seconds using UTC.
        /// </summary>
        public long Timestamp;
        public string Username;
        public string UserId;

        public Follow(string channel, string channelId, string displayName, long timestamp, string username, string userId)
        {
            Channel = channel;
            ChannelId = channelId;
            DisplayName = displayName;
            Timestamp = timestamp;
            Username = username;
            UserId = userId;
        }
    }
}