namespace StreamLogger.Api.MessageTypes
{
    public class HostingStarted
    {
        public string TargetChannel;
        public string HostingChannel;
        public int Viewers;
        public long Timestamp;
        
        public HostingStarted(string targetChannel, string hostingChannel, int viewers, long timestamp)
        {
            TargetChannel = targetChannel;
            HostingChannel = hostingChannel;
            Viewers = viewers;
            Timestamp = timestamp;
        }
    }
}