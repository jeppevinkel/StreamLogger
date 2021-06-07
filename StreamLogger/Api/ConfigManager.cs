using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StreamLogger.Api.Extensions;
using StreamLogger.Api.Interfaces;
using StreamLogger.Loader;
using StreamLogger.YamlExtensions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace StreamLogger.Api
{
    public static class ConfigManager
    {
        /// <summary>
        /// Gets the config serializer.
        /// </summary>
        private static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreFields()
            .Build();

        /// <summary>
        /// Gets the config serializer.
        /// </summary>
        private static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

        private static Dictionary<string, IConfig> _loadedConfigs = new();
        
        /// <summary>
        /// Loads all configs.
        /// </summary>
        /// <param name="rawConfigs">The raw configs to be loaded.</param>
        /// <returns>Returns a dictionary of loaded configs.</returns>
        public static Dictionary<string, IConfig> Load(string rawConfigs)
        {
            try
            {
                Log.Info("Loading configs...");

                rawConfigs = Regex.Replace(rawConfigs, @"\ !.*", string.Empty).Replace("!Dictionary[string,IConfig]", string.Empty);

                Dictionary<string, object> rawDeserializedConfigs = Deserializer.Deserialize<Dictionary<string, object>>(rawConfigs) ?? new Dictionary<string, object>();
                Dictionary<string, IConfig> deserializedConfigs = new Dictionary<string, IConfig>();

                if (!rawDeserializedConfigs.TryGetValue("Loader", out object rawDeserializedConfig))
                {
                    Log.Warn($"Loader doesn't have default configs, generating...");

                    deserializedConfigs.Add("Loader", IntegrationLoader.Config);
                }
                else
                {
                    deserializedConfigs.Add("Loader", Deserializer.Deserialize<LoaderConfig>(Serializer.Serialize(rawDeserializedConfig)));

                    IntegrationLoader.Config.CopyProperties(deserializedConfigs["Loader"]);
                }

                foreach (IIntegration<IConfig> integration in IntegrationLoader.Integrations)
                {
                    if (!rawDeserializedConfigs.TryGetValue(integration.Prefix, out rawDeserializedConfig))
                    {
                        Log.Warn($"{integration.Name} doesn't have default configs, generating...");

                        deserializedConfigs.Add(integration.Prefix, integration.Config);
                    }
                    else
                    {
                        try
                        {
                            deserializedConfigs.Add(integration.Prefix, (IConfig)Deserializer.Deserialize(Serializer.Serialize(rawDeserializedConfig), integration.Config.GetType()));

                            integration.Config.CopyProperties(deserializedConfigs[integration.Prefix]);
                        }
                        catch (YamlException yamlException)
                        {
                            Log.Error($"{integration.Name} configs could not be loaded, some of them are in a wrong format, default configs will be loaded instead! {yamlException}");

                            deserializedConfigs.Add(integration.Prefix, integration.Config);
                        }
                    }
                }

                foreach (IImplementation<IConfig> implementation in IntegrationLoader.Implementations)
                {
                    if (!rawDeserializedConfigs.TryGetValue(implementation.Prefix, out rawDeserializedConfig))
                    {
                        Log.Warn($"{implementation.Name} doesn't have default configs, generating...");

                        deserializedConfigs.Add(implementation.Prefix, implementation.Config);
                    }
                    else
                    {
                        try
                        {
                            deserializedConfigs.Add(implementation.Prefix, (IConfig)Deserializer.Deserialize(Serializer.Serialize(rawDeserializedConfig), implementation.Config.GetType()));

                            implementation.Config.CopyProperties(deserializedConfigs[implementation.Prefix]);
                        }
                        catch (YamlException yamlException)
                        {
                            Log.Error($"{implementation.Name} configs could not be loaded, some of them are in a wrong format, default configs will be loaded instead! {yamlException}");

                            deserializedConfigs.Add(implementation.Prefix, implementation.Config);
                        }
                    }
                }

                Log.Info("Configs loaded successfully!");

                _loadedConfigs = deserializedConfigs;
                return deserializedConfigs;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading configs! {exception}");

                return null;
            }
        }

        /// <summary>
        /// Saves the config of the supplied implementation to the config file.
        /// </summary>
        /// <param name="implementation">The implementation to save.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool SaveConfig(this IImplementation<IConfig> implementation)
        {
            if (!_loadedConfigs.ContainsKey(implementation.Prefix)) return false;

            _loadedConfigs[implementation.Prefix] = implementation.Config;

            return Save(_loadedConfigs);
        }

        /// <summary>
        /// Saves the config of the supplied integration to the config file.
        /// </summary>
        /// <param name="integration">The integration to save.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool SaveConfig(this IIntegration<IConfig> integration)
        {
            if (!_loadedConfigs.ContainsKey(integration.Prefix)) return false;

            _loadedConfigs[integration.Prefix] = integration.Config;

            return Save(_loadedConfigs);
        }

        /// <summary>
        /// Saves the supplied config to the config file.
        /// </summary>
        /// <param name="config">The config to save.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool Save(this IConfig config)
        {
            string configToModify = _loadedConfigs.FirstOrDefault(pair => pair.Value.ToString() == config.ToString()).Key;
            if (string.IsNullOrWhiteSpace(configToModify)) return false;

            _loadedConfigs[configToModify] = config;

            return Save(_loadedConfigs);
        }

        /// <summary>
        /// Modifies a config value and saves it to the config file.
        /// </summary>
        /// <param name="config">The config to modify.</param>
        /// <param name="property">The name of the property to modify.</param>
        /// <param name="value">The new value of the property.</param>
        /// <returns>Returns true if the operation was successful.</returns>
        public static bool SetValue<T>(this IConfig config, string property, T value)
        {
            string configToModify = _loadedConfigs.FirstOrDefault(pair => pair.Value.ToString() == config.ToString()).Key;

            if (string.IsNullOrWhiteSpace(configToModify)) return false;
            
            return _loadedConfigs[configToModify].TrySetProperty(property, value) && Save(_loadedConfigs);
        }

        /// <summary>
        /// Reads, Loads and Saves configs.
        /// </summary>
        /// <returns>Returns a value indicating if the reloading process has been completed successfully or not.</returns>
        public static bool Reload() => Save(Load(Read()));

        /// <summary>
        /// Saves configs.
        /// </summary>
        /// <param name="configs">The configs to be saved, already serialized in yaml format.</param>
        /// <returns>Returns a value indicating whether the configs have been saved successfully or not.</returns>
        private static bool Save(string configs)
        {
            try
            {
                File.WriteAllText(Paths.ConfigFile, configs ?? string.Empty);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while saving configs to {Paths.ConfigFile} path: {exception}");

                return false;
            }
        }

        /// <summary>
        /// Saves configs.
        /// </summary>
        /// <param name="configs">The configs to be saved.</param>
        /// <returns>Returns a value indicating whether the configs have been saved successfully or not.</returns>
        private static bool Save(ICollection configs)
        {
            try
            {
                if (configs == null || configs.Count == 0)
                    return false;

                return Save(Serializer.Serialize(configs));
            }
            catch (YamlException yamlException)
            {
                Log.Error($"An error has occurred while serializing configs: {yamlException}");

                return false;
            }
        }

        /// <summary>
        /// Read all configs.
        /// </summary>
        /// <returns>Returns the read configs.</returns>
        public static string Read()
        {
            try
            {
                if (File.Exists(Paths.ConfigFile))
                    return File.ReadAllText(Paths.ConfigFile);
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while reading configs from {Paths.ConfigFile} path: {exception}");
            }

            return string.Empty;
        }
    }
}