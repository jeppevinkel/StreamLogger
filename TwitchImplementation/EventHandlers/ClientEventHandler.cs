using System;
using System.Collections.Generic;
using System.Linq;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using StreamLogger.Api.MessageTypes.MiscData;
using StreamLogger.Loader;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using ChatMessage = StreamLogger.Api.MessageTypes.ChatMessage;
using Emote = StreamLogger.Api.MessageTypes.MiscData.Emote;
using HostingStarted = StreamLogger.Api.MessageTypes.HostingStarted;
using HostingStopped = StreamLogger.Api.MessageTypes.HostingStopped;

namespace TwitchImplementation.EventHandlers
{
    public static class ClientEventHandler
    {
        public static string[] DefaultColors =
        {
            "#1E90FF",
            "#FF69B4"
        };
        
        public static void OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"{e.BotUsername} - {e.Data}");
        }
        
        public static void OnConnected(object sender, OnConnectedArgs e)
        {
            Log.Info($"Connected to Twitch {e.AutoJoinChannel}");
        }

        public static void OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Log.Info($"Disconnected from Twitch");
        }
  
        public static void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Log.Info($"Joined #{e.Channel} on Twitch");
        }

        public static void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            try
            {
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(e.ChatMessage.TmiSentTs));

                HashSet<string> flags = new HashSet<string>();

                if (e.ChatMessage.IsMe)
                {
                    flags.Add("IsMe");
                }

                string colorHex = e.ChatMessage.ColorHex;

                if (string.IsNullOrEmpty(colorHex))
                {
                    colorHex = DefaultColors[IntegrationLoader.Random.Next(DefaultColors.Length)];
                }

                string avatarUrl = null;

                if (Main.Instance._bot._api is not null)
                {
                    avatarUrl = Main.Instance._bot._api.V5.Users.GetUserByIDAsync(e.ChatMessage.UserId).Result.Logo;
                }

                if (!string.IsNullOrWhiteSpace(e.ChatMessage.CustomRewardId))
                {
                    ChatMessageWithReward chatMsgWithRewards = new ChatMessageWithReward(
                        e.ChatMessage.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                        colorHex,
                        e.ChatMessage.DisplayName,
                        e.ChatMessage.EmoteSet.Emotes.ConvertAll(input =>
                            new Emote(
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
                        e.ChatMessage.Bits,
                        avatarUrl,
                        e.ChatMessage.Message,
                        e.ChatMessage.CustomRewardId);

                    ChatMessageWithRewardEventArgs messageWithRewatdEventArgs =
                        new ChatMessageWithRewardEventArgs(chatMsgWithRewards);
                    EventManager.OnChatMessageWithRewardEvent(messageWithRewatdEventArgs);
                    return;
                }

                ChatMessage chatMsg = new ChatMessage(
                    e.ChatMessage.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                    colorHex,
                    e.ChatMessage.DisplayName,
                    e.ChatMessage.EmoteSet.Emotes.ConvertAll(input => new Emote(
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
                    e.ChatMessage.UserId,
                    e.ChatMessage.Channel,
                    e.ChatMessage.Bits,
                    avatarUrl,
                    e.ChatMessage.Message);

                ChatMessageEventArgs messageEventArgs = new ChatMessageEventArgs(chatMsg);
                EventManager.OnChatMessageEvent(messageEventArgs);
            }
            catch (Exception exception)
            {
                Log.Error($"[TwitchClient] {exception}");
            }
        }

        public static void OnHostingStarted(object sender, OnHostingStartedArgs e)
        {
            HostingStartedEventArgs hostingStartedEventArgs = new HostingStartedEventArgs(
                new HostingStarted(
                    e.HostingStarted.TargetChannel,
                    e.HostingStarted.HostingChannel,
                    e.HostingStarted.Viewers,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            EventManager.OnHostingStartedEvent(hostingStartedEventArgs);
        }

        public static void OnHostingStopped(object sender, OnHostingStoppedArgs e)
        {
            HostingStoppedEventArgs hostingStoppedEventArgs = new HostingStoppedEventArgs(
                new HostingStopped(
                    e.HostingStopped.HostingChannel,
                    e.HostingStopped.Viewers,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            EventManager.OnHostingStoppedEvent(hostingStoppedEventArgs);
        }

        public static void OnBeingHosted(object sender, OnBeingHostedArgs e)
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
        
        public static void OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            // if (e.WhisperMessage.Username == "my_friend")
            //     client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }
        
        public static void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
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
            
            string avatarUrl = null;

            if (Main.Instance._bot._api is not null)
            {
                avatarUrl = Main.Instance._bot._api.V5.Users.GetUserByIDAsync(e.Subscriber.UserId).Result.Logo;
            }

            Subscription subscription = new Subscription(
                e.Subscriber.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                e.Subscriber.ColorHex,
                e.Subscriber.DisplayName,
                new EmoteSet(e.Subscriber.EmoteSet, e.Subscriber.ResubMessage).Emotes.ConvertAll(input =>
                    new Emote(input.Id,
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
                e.Subscriber.UserId,
                e.Channel,
                avatarUrl,
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
        
        public static void OnReSubscriber(object sender, OnReSubscriberArgs e)
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
            
            string avatarUrl = null;

            if (Main.Instance._bot._api is not null)
            {
                avatarUrl = Main.Instance._bot._api.V5.Users.GetUserByIDAsync(e.ReSubscriber.UserId).Result.Logo;
            }

            Subscription subscription = new Subscription(
                e.ReSubscriber.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                e.ReSubscriber.ColorHex,
                e.ReSubscriber.DisplayName,
                new EmoteSet(e.ReSubscriber.EmoteSet, e.ReSubscriber.ResubMessage).Emotes.ConvertAll(input =>
                    new Emote(input.Id,
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
                e.ReSubscriber.UserId,
                e.Channel,
                avatarUrl,
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