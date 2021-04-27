using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class ChatMessageWithRewardEventArgs : System.EventArgs
    {
        public ChatMessageWithRewardEventArgs(ChatMessageWithReward message)
        {
            Message = message;
        }

        public ChatMessageWithReward Message;
    }
}