namespace StreamLogger.Api.EventArgs
{
    public class BroadcastEventArgs : System.EventArgs
    {
        public BroadcastEventArgs(string message, string nonce = null, string returnTopic = null)
        {
            Message = message;
            Nonce = nonce;
            ReturnTopic = returnTopic;
        }

        public string Message;
        public string Nonce;
        public string ReturnTopic;
    }
}