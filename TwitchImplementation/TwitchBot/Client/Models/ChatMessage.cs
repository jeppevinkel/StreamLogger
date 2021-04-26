using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class ChatMessage
    {
        public string RawMessage = "@badge-info=subscriber/4;badges=broadcaster/1,subscriber/0,premium/1;client-nonce=a882c32cbbd3f1c706f938a402a7a4d8;color=#1E90FF;display-name=Jeppevinkel;emotes=305457542:13-28;flags=;id=b3bef2e9-dfda-4029-8593-ebf114fb68b2;mod=0;room-id=47214265;subscriber=1;tmi-sent-ts=1619379112530;turbo=0;user-id=47214265;user-type= :jeppevinkel!jeppevinkel@jeppevinkel.tmi.twitch.tv PRIVMSG #jeppevinkel :test message jeppevBouttaSnap";
        public string DisplayName;
        public string Username;
        public string UserId;
        public string UserType;
        public string RoomId;
        public string MsgId;
        public string TmiSentTs;
        public string Message;
        public string Channel;
        public string Color;
        public string CustomRewardId;
        public string Id;

        public bool IsModerator;
        public bool IsSubscriber;
        public bool IsVip;
        public bool IsBroadcaster;
        public bool IsMe;
        public bool IsStaff;
        public bool IsPartner;

        public List<KeyValuePair<string, string>> BadgeInfo;
        public List<KeyValuePair<string, string>> Badges;
        public List<Emote> Emotes;

        public int Bits;
        public int SubscribedMonths;

        public ChatMessage(IrcMessage ircMessage)
        {
            Message = ircMessage.Message;
            Channel = ircMessage.Channel;
            Username = ircMessage.User;
            RawMessage = ircMessage.RawIrc;

            foreach (KeyValuePair<string, string> tag in ircMessage.Tags)
            {
                switch (tag.Key)
                {
                    case Tags.Badges:
                        Badges = Helpers.ParseBadges(tag.Value);
                        foreach (KeyValuePair<string,string> badge in Badges)
                        {
                            switch (badge.Key)
                            {
                                case "subscriber":
                                    if (SubscribedMonths == 0)
                                    {
                                        SubscribedMonths = int.Parse(badge.Value);
                                    }
                                    break;
                                case "vip":
                                    IsVip = true;
                                    break;
                                case "admin":
                                    IsStaff = true;
                                    break;
                                case "staff":
                                    IsStaff = true;
                                    break;
                                case "partner":
                                    IsPartner = true;
                                    break;
                            }
                        }
                        break;
                    case Tags.BadgeInfo:
                        BadgeInfo = Helpers.ParseBadges(tag.Value);
                        KeyValuePair<string, string> founderBadge = BadgeInfo.Find(badge => badge.Key == "founder");
                        if (!founderBadge.Equals(default(KeyValuePair<string, string>)))
                        {
                            IsSubscriber = true;
                            SubscribedMonths = int.Parse(founderBadge.Value);
                        }
                        else
                        {
                            KeyValuePair<string, string> subBadge = BadgeInfo.Find(badge => badge.Key == "subscriber");
                            if (!subBadge.Equals(default(KeyValuePair<string, string>)))
                            {
                                SubscribedMonths = int.Parse(subBadge.Value);
                            }
                        }
                        break;
                    case Tags.Color:
                        Color = tag.Value;
                        break;
                    case Tags.DisplayName:
                        DisplayName = tag.Value;
                        break;
                    case Tags.MsgId:
                        MsgId = tag.Value;
                        break;
                    case Tags.Mod:
                        IsModerator = tag.Value == "1";
                        break;
                    case Tags.Subscriber:
                        IsSubscriber |= tag.Value == "1";
                        break;
                    case Tags.RoomId:
                        RoomId = tag.Value;
                        break;
                    case Tags.TmiSentTs:
                        TmiSentTs = tag.Value;
                        break;
                    case Tags.UserId:
                        UserId = tag.Value;
                        break;
                    case Tags.Bits:
                        Bits = int.Parse(tag.Value);
                        break;
                    case Tags.CustomRewardId:
                        CustomRewardId = tag.Value;
                        break;
                    case Tags.Id:
                        Id = tag.Value;
                        break;
                    case Tags.UserType:
                        switch (tag.Value)
                        {
                            case "mod":
                                UserType = "Moderator";
                                break;
                            case "global_mod":
                                Username = "GlobalModerator";
                                break;
                            case "admin":
                                UserType = "Admin";
                                IsStaff = true;
                                break;
                            case "staff":
                                UserType = "Staff";
                                IsStaff = true;
                                break;
                            default:
                                UserType = "Viewer";
                                break;
                        }
                        break;
                    case Tags.Emotes:
                        Emotes = Emote.Extract(tag.Value, Message).ToList();
                        break;
                }
            }
            
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = Username;

            if (Message.Length > 0 && (byte)Message[0] == 1 && (byte)Message[^1] == 1)
            {
                if (Message.StartsWith("\u0001ACTION ") && Message.EndsWith("\u0001"))
                {
                    Message = Message.Trim('\u0001')[7..];
                    IsMe = true;
                }
            }

            if (string.Equals(Channel, Username, StringComparison.InvariantCultureIgnoreCase))
            {
                IsBroadcaster = true;
            }
            if (Channel.Split(':').Length == 3)
            {
                if (string.Equals(Channel.Split(':')[1], UserId, StringComparison.InvariantCultureIgnoreCase))
                {
                    IsBroadcaster = true;
                }
            }
        }
    }
}