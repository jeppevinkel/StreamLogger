using StreamLogger.Api.Interfaces;

namespace OpenVRNotificationPipeIntegration
{
    public class Config : IConfig
    {
        public bool Enabled { get; set; } = true;

        public string PipeHost { get; set; } = "ws://localhost";
        public int PipePort { get; set; } = 8077;

        public EventToggles EnabledEvents { get; set; } = new EventToggles();
        
        public bool ShowDebugOutline { get; set; } = false;
    }

    public class EventToggles
    {
        public bool MessageEvent { get; set; } = false;
        public bool MessageWithBitsEvent { get; set; } = false;
    }
}