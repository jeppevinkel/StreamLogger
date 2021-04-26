namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class HostingStopped
    {
        public string HostingChannel;
        public int Viewers;

        public HostingStopped(IrcMessage ircMessage)
        {
            string[] split = ircMessage.Message.Split(" ");
            HostingChannel = ircMessage.Channel;
            Viewers = split[1].StartsWith("-") ? 0 : int.Parse(split[1]);
        }

        public HostingStopped(string hostingChannel, string targetChannel, int viewers)
        {
            HostingChannel = hostingChannel;
            Viewers = viewers;
        }
    }
}