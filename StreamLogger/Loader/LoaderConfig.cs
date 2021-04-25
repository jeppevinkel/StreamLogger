using StreamLogger.Api.Interfaces;

namespace StreamLogger.Loader
{
    public class LoaderConfig : IConfig
    {
        public bool Enabled { get; set; } = true;

        public bool Debug { get; set; } = false;
    }
}