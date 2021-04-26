using StreamLogger.Api;
using TwitchImplementation.TwitchBot;

namespace TwitchImplementation
{
    public class Main : Implementation<Config>
    {
        private TwitchClient _client;

        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            _client = new TwitchClient("jopodev", "hop7uuvwqdzlr4hlc0p9ogsz0i83oe");
            _client.JoinChannels(Config.Channels);
        }
    }
}