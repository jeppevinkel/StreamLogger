using System;
using System.IO;

namespace StreamLogger.Api
{
    public static class Paths
    {
        /// <summary>
        /// Gets App path.
        /// </summary>
        public static string BaseDir { get; } = AppDomain.CurrentDomain.BaseDirectory;
        
        /// <summary>
        /// Gets or sets implementations path.
        /// </summary>
        public static string Implementations { get; set; }
        
        /// <summary>
        /// Gets or sets integrations path.
        /// </summary>
        public static string Integrations { get; set; }
        
        /// <summary>
        /// Gets or sets Dependencies directory path.
        /// </summary>
        public static string Dependencies { get; set; }
        
        /// <summary>
        /// Gets or sets config directory.
        /// </summary>
        public static string ConfigDir { get; set; }
        
        /// <summary>
        /// Gets or sets config path.
        /// </summary>
        public static string ConfigFile { get; set; }
        
        /// <summary>
        /// Reloads all paths.
        /// </summary>
        public static void Reload()
        {
            Implementations = Path.Combine(BaseDir, "Implementations");
            Integrations = Path.Combine(BaseDir, "Integrations");
            Dependencies = Path.Combine(BaseDir, "Dependencies");
            // ConfigDir = Path.Combine(BaseDir, "Config");
            ConfigDir = BaseDir;
            ConfigFile = Path.Combine(ConfigDir, "Config.yml");
            // Log = Path.Combine(Exiled, $"{Server.Port}-RemoteAdminLog.txt");
        }
    }
}