using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class ReSubscriptionEventArgs : System.EventArgs
    {
        public ReSubscriptionEventArgs(Subscription subscription)
        {
            Subscription = subscription;
        }

        public Subscription Subscription;
    }
}