using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class HostNotificationEventArgs : System.EventArgs
    {
        public HostNotificationEventArgs(HostNotification hostNotification)
        {
            HostNotification = hostNotification;
        }
        
        public HostNotification HostNotification;
    }
}