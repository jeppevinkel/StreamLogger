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

            _client = new TwitchClient("jopodev", "9fq1sbem8tokz6bjwsa2duj43n2x08");
            _client.JoinChannels(Config.Channels);
        }
    }
}