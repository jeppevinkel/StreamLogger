using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using StreamLogger;
using StreamLogger.Api;
using TwitchImplementation.EventHandlers;
using TwitchImplementation.TwitchBot.Auth;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;

namespace TwitchImplementation
{
    public class Main : Implementation<Config>
    {
        // private TwitchClient _client;
        internal Bot _bot;
        internal static readonly HttpClient client = new HttpClient();
        private static Main singleton = new Main();
        
        // TODO: Implement clip events

        private Main()
        {
        }
        public static Main Instance => singleton;

        public override void Init()
        {
            base.Init();

            if (AuthenticationManager.Authenticate())
            {
                _bot = new Bot();
            }
            else
            {
                Log.Warn("Failed to validate access token. Signing in anonymously.");
                _bot = new Bot(true);
            }
        }
    }

    internal class Bot
    {
        internal readonly TwitchClient _client;
        internal readonly TwitchPubSub _pubsub; // TODO: PubSub sometimes starts throwing errors. Usually begins immediately.
        internal readonly TwitchAPI _api;
        internal readonly bool _anonymous = false;
        internal readonly Dictionary<string, string> ChannelDictionary = new Dictionary<string, string>();

        internal readonly PubSubEventHandler _pubSubEventHandler;
	
        public Bot(bool anonymous = false)
        {
            _anonymous = anonymous;

            if (!anonymous)
            {
                _api = new TwitchAPI();
                _api.Settings.ClientId = Main.Instance.Config.ClientId;
                _api.Settings.AccessToken = AuthenticationManager.TokenData.AccessToken;
            }
            
            ConnectionCredentials credentials = anonymous
                ? new ConnectionCredentials("justinfan123", "justinfan123")
                : new ConnectionCredentials(Main.Instance.Config.Username, AuthenticationManager.TokenData.AccessToken);
            
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);

            _pubSubEventHandler = new PubSubEventHandler(this);
            
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, Main.Instance.Config.Channels);

            _pubsub = new TwitchPubSub();
            
            _pubsub.OnLog += _pubSubEventHandler.OnLog;
            _pubsub.OnRewardRedeemed += _pubSubEventHandler.OnRewardRedeemed;
            _pubsub.OnPubSubServiceConnected += _pubSubEventHandler.OnPubSubServiceConnected;
            _pubsub.OnPubSubServiceError += _pubSubEventHandler.OnPubSubServiceError;
            _pubsub.OnListenResponse += _pubSubEventHandler.OnListenResponse;
            _pubsub.OnFollow += _pubSubEventHandler.OnFollow;

            Task.Run(async () =>
            {
                GetUsersResponse users = await _api.Helix.Users.GetUsersAsync(null, Main.Instance.Config.Channels);

                foreach (User user in users.Users)
                {
                    ChannelDictionary.Add(user.Id, user.Login);
                    
                    _pubsub.ListenToRewards(user.Id);
                    _pubsub.ListenToFollows(user.Id);
                }
            });

            _pubsub.Connect();

            _client.OnLog += ClientEventHandler.OnLog;
            _client.OnJoinedChannel += ClientEventHandler.OnJoinedChannel;
            _client.OnMessageReceived += ClientEventHandler.OnMessageReceived;
            _client.OnWhisperReceived += ClientEventHandler.OnWhisperReceived;
            _client.OnNewSubscriber += ClientEventHandler.OnNewSubscriber;
            _client.OnReSubscriber += ClientEventHandler.OnReSubscriber;
            _client.OnConnected += ClientEventHandler.OnConnected;
            _client.OnDisconnected += ClientEventHandler.OnDisconnected;
            _client.OnHostingStarted += ClientEventHandler.OnHostingStarted;
            _client.OnHostingStopped += ClientEventHandler.OnHostingStopped;
            _client.OnBeingHosted += ClientEventHandler.OnBeingHosted;

            _client.Connect();
        }
  
        
    }
}