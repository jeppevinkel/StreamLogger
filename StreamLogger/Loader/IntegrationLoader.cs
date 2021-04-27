using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StreamLogger.Api;
using StreamLogger.Api.Interfaces;

namespace StreamLogger.Loader
{
    public static class IntegrationLoader
    {
        static IntegrationLoader()
        {
            Log.Info($"Initializing at {Environment.CurrentDirectory}");
            Log.Info($"{Assembly.GetExecutingAssembly().GetName().Name} - Version {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            
            Paths.Reload();
            
            if (!Directory.Exists(Paths.ConfigDir))
                Directory.CreateDirectory(Paths.ConfigDir);

            if (!Directory.Exists(Paths.Implementations))
                Directory.CreateDirectory(Paths.Implementations);

            if (!Directory.Exists(Paths.Integrations))
                Directory.CreateDirectory(Paths.Integrations);

            if (!Directory.Exists(Paths.Dependencies))
                Directory.CreateDirectory(Paths.Dependencies);
        }
        
        /// <summary>
        /// Gets the integrations list.
        /// </summary>
        public static List<IIntegration<IConfig>> Integrations { get; } = new List<IIntegration<IConfig>>();
        
        /// <summary>
        /// Gets the implementations list.
        /// </summary>
        public static List<IImplementation<IConfig>> Implementations { get; } = new List<IImplementation<IConfig>>();
        
        /// <summary>
        /// Gets a dictionary containing the file paths of assemblies.
        /// </summary>
        public static Dictionary<Assembly, string> Locations { get; } = new Dictionary<Assembly, string>();
        
        /// <summary>
        /// Gets the initialized global random class.
        /// </summary>
        public static Random Random { get; } = new Random();
        
        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        
        /// <summary>
        /// Gets the configs of the integration manager.
        /// </summary>
        public static LoaderConfig Config { get; } = new LoaderConfig();
        
        /// <summary>
        /// Gets integration dependencies.
        /// </summary>
        public static List<Assembly> Dependencies { get; } = new List<Assembly>();
        
        /// <summary>
        /// Runs the integration manager, by loading all dependencies, integrations, configs and then enables all plugins.
        /// </summary>
        public static void Run()
        {
            ConfigManager.Load(ConfigManager.Read());
            Log.DebugMode = Config.Debug;
            LoadDependencies();
            LoadIntegrations();
            LoadImplementations();

            ConfigManager.Reload();

            EnableIntegrations();
            EnableImplementations();
        }
        
        /// <summary>
        /// Loads all integrations.
        /// </summary>
        public static void LoadIntegrations()
        {
            foreach (string assemblyPath in Directory.GetFiles(Paths.Integrations, "*.dll"))
            {
                Assembly assembly = LoadAssembly(assemblyPath);

                if (assembly == null)
                    continue;

                Locations[assembly] = assemblyPath;
            }

            foreach (Assembly assembly in Locations.Keys)
            {
                if (!Locations[assembly].Contains(Paths.Integrations))
                    continue;

                IIntegration<IConfig> integration = CreateIntegration(assembly);

                if (integration == null)
                    continue;

                Integrations.Add(integration);
            }
        }
        
        /// <summary>
        /// Loads all implementations.
        /// </summary>
        public static void LoadImplementations()
        {
            foreach (string assemblyPath in Directory.GetFiles(Paths.Implementations, "*.dll"))
            {
                Assembly assembly = LoadAssembly(assemblyPath);

                if (assembly == null)
                    continue;

                Locations[assembly] = assemblyPath;
            }

            foreach (Assembly assembly in Locations.Keys)
            {
                if (!Locations[assembly].Contains(Paths.Implementations))
                    continue;

                IImplementation<IConfig> implementation = CreateImplementation(assembly);

                if (implementation == null)
                    continue;

                Implementations.Add(implementation);
            }
        }
        
        /// <summary>
        /// Loads an assembly.
        /// </summary>
        /// <param name="path">The path to load the assembly from.</param>
        /// <returns>Returns the loaded assembly or null.</returns>
        public static Assembly LoadAssembly(string path)
        {
            try
            {
                return Assembly.Load(File.ReadAllBytes(path));
            }
            catch (Exception exception)
            {
                Log.Error($"Error while loading an assembly at {path}! {exception}");
            }

            return null;
        }
        
        /// <summary>
        /// Create an integration instance.
        /// </summary>
        /// <param name="assembly">The integration assembly.</param>
        /// <returns>Returns the created integration instance or null.</returns>
        public static IIntegration<IConfig> CreateIntegration(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Integration<>))
                    {
                        Log.Debug($"\"{type.FullName}\" does not inherit from Integration<TConfig>, skipping.");
                        continue;
                    }

                    Log.Debug($"Loading type {type.FullName}");

                    IIntegration<IConfig> integration = null;

                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        Log.Debug("Public default constructor found, creating instance...");

                        integration = constructor.Invoke(null) as IIntegration<IConfig>;
                    }
                    else
                    {
                        Log.Debug($"Constructor wasn't found, searching for a property with the {type.FullName} type...");

                        var value = Array.Find(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), property => property.PropertyType == type)?.GetValue(null);

                        if (value != null)
                            integration = value as IIntegration<IConfig>;
                    }

                    if (integration == null)
                    {
                        Log.Error($"{type.FullName} is a valid integration, but it cannot be instantiated! It either doesn't have a public default constructor without any arguments or a static property of the {type.FullName} type!");

                        continue;
                    }

                    Log.Debug($"Instantiated type {type.FullName}");

                    return integration;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error while initializing integration {assembly.GetName().Name} (at {assembly.Location})! {exception}");
            }

            return null;
        }
        
        /// <summary>
        /// Create an implementation instance.
        /// </summary>
        /// <param name="assembly">The implementation assembly.</param>
        /// <returns>Returns the created implementation instance or null.</returns>
        public static IImplementation<IConfig> CreateImplementation(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Implementation<>))
                    {
                        Log.Debug($"\"{type.FullName}\" does not inherit from Implementation<TConfig>, skipping.");
                        continue;
                    }

                    Log.Debug($"Loading type {type.FullName}");

                    IImplementation<IConfig> implementation = null;

                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        Log.Debug("Public default constructor found, creating instance...");

                        implementation = constructor.Invoke(null) as IImplementation<IConfig>;
                    }
                    else
                    {
                        Log.Debug($"Constructor wasn't found, searching for a property with the {type.FullName} type...");

                        var value = Array.Find(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), property => property.PropertyType == type)?.GetValue(null);

                        if (value != null)
                            implementation = value as IImplementation<IConfig>;
                    }

                    if (implementation == null)
                    {
                        Log.Error($"{type.FullName} is a valid implementation, but it cannot be instantiated! It either doesn't have a public default constructor without any arguments or a static property of the {type.FullName} type!");

                        continue;
                    }

                    Log.Debug($"Instantiated type {type.FullName}");

                    return implementation;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error while initializing implementation {assembly.GetName().Name} (at {assembly.Location})! {exception}");
            }

            return null;
        }
        
        /// <summary>
        /// Enables all integrations.
        /// </summary>
        public static void EnableIntegrations()
        {
            foreach (IIntegration<IConfig> integration in Integrations)
            {
                try
                {
                    if (integration.Config.Enabled)
                    {
                        integration.Init();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Integration \"{integration.Name}\" threw an exception while enabling: {exception}");
                }
            }
        }
        
        /// <summary>
        /// Enables all implementations.
        /// </summary>
        public static void EnableImplementations()
        {
            foreach (IImplementation<IConfig> implementation in Implementations)
            {
                try
                {
                    if (implementation.Config.Enabled)
                    {
                        implementation.Init();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Implementation \"{implementation.Name}\" threw an exception while enabling: {exception}");
                }
            }
        }
        
        /// <summary>
        /// Loads all dependencies.
        /// </summary>
        private static void LoadDependencies()
        {
            try
            {
                Log.Info($"Loading dependencies at {Paths.Dependencies}");

                foreach (string dependency in Directory.GetFiles(Paths.Dependencies, "*.dll"))
                {
                    Assembly assembly = LoadAssembly(dependency);

                    if (assembly == null)
                        continue;

                    Locations[assembly] = dependency;

                    Dependencies.Add(assembly);

                    Log.Info($"Loaded dependency {assembly.FullName}");
                }

                Log.Info("Dependencies loaded successfully!");
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading dependencies! {exception}");
            }
        }
    }
}