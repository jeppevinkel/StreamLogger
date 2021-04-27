using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class NewSubscriptionEventArgs : System.EventArgs
    {
        public NewSubscriptionEventArgs(Subscription subscription)
        {
            Subscription = subscription;
        }

        public Subscription Subscription;
    }
}