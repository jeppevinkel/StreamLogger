using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using StreamLogger.Api.MessageTypes.MiscData;
using TwitchImplementation.EventHandlers;
using TwitchImplementation.TwitchBot.Auth;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using OnLogArgs = TwitchLib.Client.Events.OnLogArgs;

namespace TwitchImplementation
{
    public class Main : Implementation<Config>
    {
        // private TwitchClient _client;
        internal Bot _bot;
        internal static readonly HttpClient client = new HttpClient();
        private static Main singleton = new Main();

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
        internal readonly TwitchPubSub _pubsub;
        internal readonly bool _anonymous = false;
	
        public Bot(bool anonymous = false)
        {
            _anonymous = anonymous;
            
            ConnectionCredentials credentials = anonymous
                ? new ConnectionCredentials("justinfan123", "justinfan123")
                : new ConnectionCredentials(Main.Instance.Config.Username, AuthenticationManager.TokenData.AccessToken);
            
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, Main.Instance.Config.Channels);

            _pubsub = new TwitchPubSub();
            
            _pubsub.OnLog += PubSubEventHandler.OnLog;
            _pubsub.OnRewardRedeemed += PubSubEventHandler.OnRewardRedeemed;
            _pubsub.OnPubSubServiceConnected += PubSubEventHandler.OnPubSubServiceConnected;
            _pubsub.OnPubSubServiceError += PubSubEventHandler.OnPubSubServiceError;
            _pubsub.OnListenResponse += PubSubEventHandler.OnListenResponse;
            
            _pubsub.ListenToRewards("47214265"); //TODO Make automatic channel id resolution.
            
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