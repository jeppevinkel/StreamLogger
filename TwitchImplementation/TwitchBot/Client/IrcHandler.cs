using System;
using System.Collections.Generic;
using System.Linq;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using TwitchImplementation.TwitchBot.Client.Enums;
using TwitchImplementation.TwitchBot.Client.Models;
using ChatMessage = TwitchImplementation.TwitchBot.Client.Models.ChatMessage;
using Emote = StreamLogger.Api.MessageTypes.MiscData.Emote;
using HostingStarted = TwitchImplementation.TwitchBot.Client.Models.HostingStarted;
using HostingStopped = TwitchImplementation.TwitchBot.Client.Models.HostingStopped;

namespace TwitchImplementation.TwitchBot.Client
{
    public class IrcHandler
    {
        public static void HandleIrc(IrcMessage ircMessage)
        {
            if (ircMessage.Message.Contains("Login authentication failed"))
            {
                Log.Error(ircMessage.Message);
            }

            switch (ircMessage.Command)
            {
                case IrcCommand.PrivMsg:
                    HandlePrivMsg(ircMessage);
                    break;
                case IrcCommand.HostTarget:
                    HandleHostTarget(ircMessage);
                    break;
                case IrcCommand.Unknown:
                    Log.Warn($"Unaccounted for: {ircMessage.RawIrc}");
                    break;
            }
        }

        private static void HandlePrivMsg(IrcMessage ircMessage)
        {
            if (ircMessage.Hostmask.Equals("jtv!jtv@jtv.tmi.twitch.tv"))
            {
                HostedNotification hostedNotification = new HostedNotification(ircMessage);
                HostNotification hostNotification = new HostNotification(
                    hostedNotification.Channel,
                    hostedNotification.HostedByChannel,
                    hostedNotification.Viewers,
                    hostedNotification.IsAutoHost,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                HostNotificationEventArgs hostNotificationEventArgs = new HostNotificationEventArgs(hostNotification);
                EventManager.OnHostNotificationEvent(hostNotificationEventArgs);
                return;
            }
            
            ChatMessage chatMessage = new ChatMessage(ircMessage);

            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(chatMessage.TmiSentTs));

            HashSet<string> flags = new HashSet<string>();

            if (chatMessage.IsMe)
            {
                flags.Add("IsMe");
            }

            StreamLogger.Api.MessageTypes.ChatMessage chatMsg = new StreamLogger.Api.MessageTypes.ChatMessage(
                chatMessage.Badges.ToDictionary(b => b.Key, b => int.Parse(b.Value)),
                chatMessage.Color,
                chatMessage.DisplayName,
                chatMessage.Emotes.ConvertAll(input => new Emote(input.Id,
                    input.StartIndex,
                    input.EndIndex,
                    input.Name,
                    input.ImageUrl)),
                flags,
                chatMessage.IsModerator,
                chatMessage.IsSubscriber,
                chatMessage.IsBroadcaster,
                dto.ToUnixTimeSeconds(),
                chatMessage.UserType,
                chatMessage.Username,
                chatMessage.UserId,
                chatMessage.Channel,
                chatMessage.Bits,
                null,
                chatMessage.Message);

            ChatMessageEventArgs messageEventArgs = new ChatMessageEventArgs(chatMsg);
            EventManager.OnChatMessageEvent(messageEventArgs);
        }

        private static void HandleHostTarget(IrcMessage ircMessage)
        {
            if (ircMessage.Message.StartsWith("-"))
            {
                var hostingStopped = new HostingStopped(ircMessage);

                HostingStoppedEventArgs hostingStoppedEventArgs = new HostingStoppedEventArgs(
                    new StreamLogger.Api.MessageTypes.HostingStopped(
                        hostingStopped.HostingChannel,
                        hostingStopped.Viewers,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                EventManager.OnHostingStoppedEvent(hostingStoppedEventArgs);
            }
            else
            {
                var hostingStarted = new HostingStarted(ircMessage);

                HostingStartedEventArgs hostingStartedEventArgs = new HostingStartedEventArgs(
                    new StreamLogger.Api.MessageTypes.HostingStarted(
                        hostingStarted.TargetChannel,
                        hostingStarted.HostingChannel,
                        hostingStarted.Viewers,
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                EventManager.OnHostingStartedEvent(hostingStartedEventArgs);
            }
        }
    }
}