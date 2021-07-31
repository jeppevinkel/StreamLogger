using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace TextToSpeechIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;

        [Description("The API key from the google cloud console. Located at https://console.cloud.google.com/apis/credentials after you create a project and enable the API.")]
        public string ApiKey { get; set; } = "";
        
        [Description("Sets the program to read out all chat messages.")]
        public bool ReadAllChat { get; set; } = true;
        
        [Description("Id of the reward to use if not set to read all text.")]
        public string RewardId { get; set; } = "";

        [Description("Sets the program to read out the names of the chatters.")]
        public bool MentionNames { get; set; } = true;

        [Description("Sets the program to clean out names before reading them.")]
        public bool UseReadableNames { get; set; } = true;

        [Description("Sets the program to remove emotes before reading the message.")]
        public bool StripEmotes { get; set; } = true;
        
        [Description("Enable to prevent messages from the broadcaster from showing up.")]
        public bool IgnoreBroadcaster { get; set; } = false;

        [Description("Messages starting with any of the prefixes listed here will get ignored.")]
        public string[] IgnorePrefix { get; set; } = new[] {"!"};

        [Description("Messages starting with this prefix are parsed as commands.")]
        public string CommandPrefix { get; set; } = "!";
    }
}