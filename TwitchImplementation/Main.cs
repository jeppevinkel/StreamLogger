using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using StreamLogger.Api.MessageTypes.MiscData;
using TwitchImplementation.TwitchBot.Auth;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchImplementation
{
    public class Main : Implementation<Config>
    {
        // private TwitchClient _client;
        Bot _bot;
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
        readonly TwitchClient client;
	
        public Bot(bool anonymous = false)
        {
            ConnectionCredentials credentials = anonymous
                ? new ConnectionCredentials("justinfan123", "justinfan123")
                : new ConnectionCredentials(Main.Instance.Config.Username, AuthenticationManager.TokenData.AccessToken);
            
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, Main.Instance.Config.Channels[0]);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnReSubscriber += Client_OnReSubscriber;
            client.OnConnected += Client_OnConnected;
            client.OnHostingStarted += Client_OnHostingStarted;
            client.OnHostingStopped += Client_OnHostingStopped;
            client.OnBeingHosted += Client_OnBeingHosted;

            client.Connect();
        }
  
        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"{e.BotUsername} - {e.Data}");
        }
  
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Log.Info($"Connected to Twitch {e.AutoJoinChannel}");
            foreach (string channel in Main.Instance.Config.Channels.Skip(1))
            {
                client.JoinChannel(channel);
            }
        }
  
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Log.Info($"Joined #{e.Channel} on Twitch");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(e.ChatMessage.TmiSentTs));

            HashSet<string> flags = new HashSet<string>();

            if (e.ChatMessage.IsMe)
            {
                flags.Add("IsMe");
            }

            if (!string.IsNullOrWhiteSpace(e.ChatMessage.CustomRewardId))
            {
                ChatMessageWithReward chatMsgWithRewards = new ChatMessageWithReward(
                    e.ChatMessage.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                    e.ChatMessage.ColorHex,
                    e.ChatMessage.DisplayName,
                    e.ChatMessage.EmoteSet.Emotes.ConvertAll(input => new StreamLogger.Api.MessageTypes.MiscData.Emote(
                        input.Id,
                        input.StartIndex,
                        input.EndIndex,
                        input.Name,
                        input.ImageUrl)),
                    flags,
                    e.ChatMessage.IsModerator,
                    e.ChatMessage.IsSubscriber,
                    e.ChatMessage.IsBroadcaster,
                    dto.ToUnixTimeSeconds(),
                    e.ChatMessage.UserType.ToString(),
                    e.ChatMessage.Username,
                    e.ChatMessage.Channel,
                    e.ChatMessage.Message,
                    e.ChatMessage.CustomRewardId);
                
                ChatMessageWithRewardEventArgs messageWithRewatdEventArgs = new ChatMessageWithRewardEventArgs(chatMsgWithRewards);
                EventManager.OnChatMessageWithRewardEvent(messageWithRewatdEventArgs);
                return;
            }
                
            StreamLogger.Api.MessageTypes.ChatMessage chatMsg = new StreamLogger.Api.MessageTypes.ChatMessage(
                e.ChatMessage.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                e.ChatMessage.ColorHex,
                e.ChatMessage.DisplayName,
                e.ChatMessage.EmoteSet.Emotes.ConvertAll(input => new StreamLogger.Api.MessageTypes.MiscData.Emote(input.Id,
                    input.StartIndex,
                    input.EndIndex,
                    input.Name,
                    input.ImageUrl)),
                flags,
                e.ChatMessage.IsModerator,
                e.ChatMessage.IsSubscriber,
                e.ChatMessage.IsBroadcaster,
                dto.ToUnixTimeSeconds(),
                e.ChatMessage.UserType.ToString(),
                e.ChatMessage.Username,
                e.ChatMessage.Channel,
                e.ChatMessage.Message);
            
            ChatMessageEventArgs messageEventArgs = new ChatMessageEventArgs(chatMsg);
            EventManager.OnChatMessageEvent(messageEventArgs);
        }

        private void Client_OnHostingStarted(object sender, OnHostingStartedArgs e)
        {
            HostingStartedEventArgs hostingStartedEventArgs = new HostingStartedEventArgs(
                new StreamLogger.Api.MessageTypes.HostingStarted(
                    e.HostingStarted.TargetChannel,
                    e.HostingStarted.HostingChannel,
                    e.HostingStarted.Viewers,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            EventManager.OnHostingStartedEvent(hostingStartedEventArgs);
        }

        private void Client_OnHostingStopped(object sender, OnHostingStoppedArgs e)
        {
            HostingStoppedEventArgs hostingStoppedEventArgs = new HostingStoppedEventArgs(
                new StreamLogger.Api.MessageTypes.HostingStopped(
                    e.HostingStopped.HostingChannel,
                    e.HostingStopped.Viewers,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            EventManager.OnHostingStoppedEvent(hostingStoppedEventArgs);
        }

        private void Client_OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            HostNotification hostNotification = new HostNotification(
                e.BeingHostedNotification.Channel,
                e.BeingHostedNotification.HostedByChannel,
                e.BeingHostedNotification.Viewers,
                e.BeingHostedNotification.IsAutoHosted,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            HostNotificationEventArgs hostNotificationEventArgs = new HostNotificationEventArgs(hostNotification);
            EventManager.OnHostNotificationEvent(hostNotificationEventArgs);
        }
        
        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            // if (e.WhisperMessage.Username == "my_friend")
            //     client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }
        
        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(e.Subscriber.TmiSentTs));

            HashSet<string> flags = new HashSet<string>();

            SubscriptionPlan subPlan = SubscriptionPlan.NotSet;

            subPlan = e.Subscriber.SubscriptionPlan switch
            {
                TwitchLib.Client.Enums.SubscriptionPlan.NotSet => SubscriptionPlan.NotSet,
                TwitchLib.Client.Enums.SubscriptionPlan.Prime => SubscriptionPlan.Prime,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier1 => SubscriptionPlan.Tier1,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier2 => SubscriptionPlan.Tier2,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier3 => SubscriptionPlan.Tier3,
                _ => subPlan
            };

            Subscription subscription = new Subscription(
                e.Subscriber.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                e.Subscriber.ColorHex,
                e.Subscriber.DisplayName,
                new EmoteSet(e.Subscriber.EmoteSet, e.Subscriber.ResubMessage).Emotes.ConvertAll(input =>
                    new StreamLogger.Api.MessageTypes.MiscData.Emote(input.Id,
                        input.StartIndex,
                        input.EndIndex,
                        input.Name,
                        input.ImageUrl)),
                flags,
                e.Subscriber.IsModerator,
                e.Subscriber.IsSubscriber,
                dto.ToUnixTimeSeconds(),
                e.Subscriber.UserType.ToString(),
                e.Subscriber.Login,
                e.Channel,
                e.Subscriber.ResubMessage,
                subPlan,
                e.Subscriber.SubscriptionPlanName,
                int.Parse(e.Subscriber.MsgParamCumulativeMonths),
                e.Subscriber.MsgParamShouldShareStreak,
                int.Parse(e.Subscriber.MsgParamStreakMonths),
                e.Subscriber.SystemMessageParsed);
            
            NewSubscriptionEventArgs newSubscriptionEventArgs = new NewSubscriptionEventArgs(subscription);
            EventManager.OnNewSubscriptionEvent(newSubscriptionEventArgs);
        }
        
        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(e.ReSubscriber.TmiSentTs));

            HashSet<string> flags = new HashSet<string>();

            SubscriptionPlan subPlan = SubscriptionPlan.NotSet;

            subPlan = e.ReSubscriber.SubscriptionPlan switch
            {
                TwitchLib.Client.Enums.SubscriptionPlan.NotSet => SubscriptionPlan.NotSet,
                TwitchLib.Client.Enums.SubscriptionPlan.Prime => SubscriptionPlan.Prime,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier1 => SubscriptionPlan.Tier1,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier2 => SubscriptionPlan.Tier2,
                TwitchLib.Client.Enums.SubscriptionPlan.Tier3 => SubscriptionPlan.Tier3,
                _ => subPlan
            };

            Subscription subscription = new Subscription(
                e.ReSubscriber.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                e.ReSubscriber.ColorHex,
                e.ReSubscriber.DisplayName,
                new EmoteSet(e.ReSubscriber.EmoteSet, e.ReSubscriber.ResubMessage).Emotes.ConvertAll(input =>
                    new StreamLogger.Api.MessageTypes.MiscData.Emote(input.Id,
                        input.StartIndex,
                        input.EndIndex,
                        input.Name,
                        input.ImageUrl)),
                flags,
                e.ReSubscriber.IsModerator,
                e.ReSubscriber.IsSubscriber,
                dto.ToUnixTimeSeconds(),
                e.ReSubscriber.UserType.ToString(),
                e.ReSubscriber.Login,
                e.Channel,
                e.ReSubscriber.ResubMessage,
                subPlan,
                e.ReSubscriber.SubscriptionPlanName,
                int.Parse(e.ReSubscriber.MsgParamCumulativeMonths),
                e.ReSubscriber.MsgParamShouldShareStreak,
                int.Parse(e.ReSubscriber.MsgParamStreakMonths),
                e.ReSubscriber.SystemMessageParsed);
            
            ReSubscriptionEventArgs reSubscriptionEventArgs = new ReSubscriptionEventArgs(subscription);
            EventManager.OnReSubscriptionEvent(reSubscriptionEventArgs);
        }
    }
}