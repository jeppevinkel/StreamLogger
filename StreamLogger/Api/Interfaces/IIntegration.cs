using System;
using System.Reflection;

namespace StreamLogger.Api.Interfaces
{
    public interface IIntegration<out TConfig> where TConfig : IConfig
    {
        Assembly Assembly { get; }
        
        /// <summary>
        /// Gets the integration name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the integration prefix.
        /// </summary>
        string Prefix { get; }
        
        /// <summary>
        /// Gets the integration version.
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// Gets the integration config.
        /// </summary>
        TConfig Config { get; }
        
        /// <summary>
        /// Initializes the integration.
        /// </summary>
        public void Init();
    }
}