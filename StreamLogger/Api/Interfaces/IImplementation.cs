using System;
using System.Reflection;

namespace StreamLogger.Api.Interfaces
{
    public interface IImplementation<out TConfig> where TConfig : IConfig
    {
        Assembly Assembly { get; }
        
        /// <summary>
        /// Gets the implementation name.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the implementation prefix.
        /// </summary>
        string Prefix { get; }
        
        /// <summary>
        /// Gets the implementation version.
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// Gets the implementation config.
        /// </summary>
        TConfig Config { get; }
        
        /// <summary>
        /// Initializes the implementation.
        /// </summary>
        public void Init();
    }
}