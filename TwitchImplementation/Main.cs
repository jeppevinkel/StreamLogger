using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
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
        }
    }

    internal class Bot
    {
        readonly TwitchClient client;
	
        public Bot()
        {
            var credentials = new ConnectionCredentials(Main.Instance.Config.Username, AuthenticationManager.TokenData.AccessToken);
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
            // if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            //     client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            // else
            //     client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }
    }
}