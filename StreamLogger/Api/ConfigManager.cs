using System;
using System.IO;
using StreamLogger.YamlExtensions;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StreamLogger.Api.Extensions;
using StreamLogger.Api.Interfaces;
using StreamLogger.Loader;
using YamlDotNet.Core;
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

                return deserializedConfigs;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading configs! {exception}");

                return null;
            }
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
        public static bool Save(string configs)
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
        public static bool Save(Dictionary<string, IConfig> configs)
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