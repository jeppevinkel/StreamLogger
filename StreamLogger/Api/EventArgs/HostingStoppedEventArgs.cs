using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class HostingStoppedEventArgs : System.EventArgs
    {
        public HostingStoppedEventArgs(HostingStopped hostingStopped)
        {
            HostingStopped = hostingStopped;
        }
        
        public HostingStopped HostingStopped;
    }
}