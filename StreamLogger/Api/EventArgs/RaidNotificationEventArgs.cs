using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class RaidNotificationEventArgs : System.EventArgs
    {
        public RaidNotificationEventArgs(RaidNotification raidNotification)
        {
            RaidNotification = raidNotification;
        }

        public RaidNotification RaidNotification;
    }
}