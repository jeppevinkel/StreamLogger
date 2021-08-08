using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace YouTubeImplementation
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;

        [Description("OAuth credentials obtained from https://console.cloud.google.com/apis/")]
        public string ClientId { get; set; } = "";

        [Description("OAuth credentials obtained from https://console.cloud.google.com/apis/")]
        public string ClientSecret { get; set; } = "";
    }
}