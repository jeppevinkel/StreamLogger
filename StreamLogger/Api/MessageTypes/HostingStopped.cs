namespace StreamLogger.Api.MessageTypes
{
    public class HostingStopped
    {
        public string HostingChannel;
        public int Viewers;
        public long Timestamp;
        
        public HostingStopped(string hostingChannel, int viewers, long timestamp)
        {
            HostingChannel = hostingChannel;
            Viewers = viewers;
            Timestamp = timestamp;
        }
    }
}