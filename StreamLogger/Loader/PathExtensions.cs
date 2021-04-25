using System;
using System.Reflection;
using StreamLogger.Api.Interfaces;

namespace StreamLogger.Loader
{
    public static class PathExtensions
    {
        public static string GetPath(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            IntegrationLoader.Locations.TryGetValue(assembly, out var path);
            return path;
        }
        
        public static string GetPath(this IIntegration<IConfig> integration)
        {
            if (integration == null)
                throw new ArgumentNullException(nameof(integration));

            return integration.Assembly.GetPath();
        }
    }
}