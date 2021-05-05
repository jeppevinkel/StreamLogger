using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class FollowEventArgs : System.EventArgs
    {
        public FollowEventArgs(Follow follow)
        {
            Follow = follow;
        }

        public Follow Follow;
    }
}