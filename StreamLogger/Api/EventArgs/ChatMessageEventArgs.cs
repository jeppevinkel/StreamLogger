using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class ChatMessageEventArgs : System.EventArgs
    {
        public ChatMessageEventArgs(ChatMessage message)
        {
            Message = message;
        }

        public ChatMessage Message;
    }
}