namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class HostingStarted
    {
        public string HostingChannel;
        public string TargetChannel;
        public int Viewers;

        public HostingStarted(IrcMessage ircMessage)
        {
            string[] split = ircMessage.Message.Split(" ");
            HostingChannel = ircMessage.Channel;
            TargetChannel = split[0];
            Viewers = split[1].StartsWith("-") ? 0 : int.Parse(split[1]);
        }

        public HostingStarted(string hostingChannel, string targetChannel, int viewers)
        {
            HostingChannel = hostingChannel;
            TargetChannel = targetChannel;
            Viewers = viewers;
        }
    }
}