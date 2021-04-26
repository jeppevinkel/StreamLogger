using System;
using System.Linq;

namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class HostedNotification
    {
        public string RawMessage;
        public string Channel;
        public string HostedByChannel;
        public bool IsAutoHost;
        public int Viewers;

        public HostedNotification(IrcMessage ircMessage)
        {
            Channel = ircMessage.Channel;
            HostedByChannel = ircMessage.Message.Split(" ").First();
            RawMessage = ircMessage.RawIrc;

            if (ircMessage.Message.Contains("up to "))
            {
                string[] split = ircMessage.Message.Split(new[] {"up to "}, StringSplitOptions.None);
                if (split[1].Contains(" ") && int.TryParse(split[1].Split(" ")[0], out int n))
                {
                    Viewers = n;
                }
            }

            if (ircMessage.Message.Contains("auto hosting"))
            {
                IsAutoHost = true;
            }
        }

        public HostedNotification(string channel, string hostedByChannel, int viewers, bool isAutoHost)
        {
            Channel = channel;
            HostedByChannel = hostedByChannel;
            Viewers = viewers;
            IsAutoHost = isAutoHost;
        }
    }
}