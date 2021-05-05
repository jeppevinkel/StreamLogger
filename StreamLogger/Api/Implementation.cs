using System;
using System.Reflection;
using StreamLogger.Api.Extensions;
using StreamLogger.Api.Interfaces;

namespace StreamLogger.Api
{
    public abstract class Implementation<TConfig> : IImplementation<TConfig> where TConfig : IConfig, new()
    {
        public Implementation()
        {
            Name = Assembly.GetName().Name;
            Prefix = Name.ToPascalCase();
            Version = Assembly.GetName().Version;
        }
        
        /// <inheritdoc/>
        public Assembly Assembly { get; } = Assembly.GetCallingAssembly();
        
        /// <inheritdoc/>
        public virtual string Name { get; }
        
        /// <inheritdoc/>
        public virtual string Prefix { get; }
        
        /// <inheritdoc/>
        public virtual Version Version { get; }
        
        /// <inheritdoc/>
        public TConfig Config { get; } = new TConfig();

        /// <inheritdoc/>
        public virtual void Init()
        {
            var attribute = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Log.Info($"{Name} v{(attribute == null ? $"{Version.Major}.{Version.Minor}.{Version.Build}" : attribute.InformationalVersion)} has been enabled!");
        }
    }
}