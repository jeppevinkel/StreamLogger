using System.ComponentModel;
using StreamLogger.Api.Interfaces;

namespace SuperScreenShotterVRIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = false;
        
        public string Host { get; set; } = "ws://localhost";
        public int Port { get; set; } = 8807;

        [Description("Webhook used for Discord messages.")]
        public string Webhook { get; set; } = "";

        [Description("The description shown above the image embed. {message} will get replaced with the message written by the redeemer.")]
        public string ImageDescription { get; set; } = "Photograph: {message}";

        [Description("The ID of the reward to trigger a screenshot.")]
        public string RewardId { get; set; } = "";

        [Description("Delay in seconds for screenshot posing.")]
        public int Delay { get; set; } = 2;
    }
}