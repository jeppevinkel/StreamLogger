using StreamLogger.Api.MessageTypes;

namespace StreamLogger.Api.EventArgs
{
    public class GameChangeEventArgs : System.EventArgs
    {
        public GameChangeEventArgs(GameChange gameChange)
        {
            GameChange = gameChange;
        }

        public GameChange GameChange;
    }
}