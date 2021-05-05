using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class RewardEventArgs : System.EventArgs
    {
        public RewardEventArgs(Reward reward)
        {
            Reward = reward;
        }

        public Reward Reward;
    }
}