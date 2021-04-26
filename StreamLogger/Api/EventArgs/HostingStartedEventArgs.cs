using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class HostingStartedEventArgs : System.EventArgs
    {
        public HostingStartedEventArgs(HostingStarted hostingStarted)
        {
            HostingStarted = hostingStarted;
        }
        
        public HostingStarted HostingStarted;
    }
}