namespace StreamLogger.Api.MessageTypes
{
    public class HostNotification
    {
        public string TargetChannel;
        public string HostingChannel;
        public bool IsAutoHost;
        public int Viewers;
        public long Timestamp;
        
        public HostNotification(string targetChannel, string hostingChannel, int viewers, bool isAutoHost, long timestamp)
        {
            TargetChannel = targetChannel;
            HostingChannel = hostingChannel;
            Viewers = viewers;
            IsAutoHost = isAutoHost;
            Timestamp = timestamp;
        }
    }
}