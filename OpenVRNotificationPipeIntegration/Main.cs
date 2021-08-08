using System;
using System.IO;
using OpenVRNotificationPipeIntegration.EventHandlers;
using StreamLogger;
using StreamLogger.Api;

namespace OpenVRNotificationPipeIntegration
{
    public sealed class Main : Integration<Config>
    {
        private static Main singleton = new Main();

        private Main()
        {
            BaseDir = Path.Combine(Paths.Integrations, Name);
        }
        public static Main Instance => singleton;
        public static Config MyConfig => singleton.Config;
        
        public readonly string BaseDir;
        public StyleManager StyleManager;
        public PipeManager PipeManager;

        public string CurrentGame = "";
        
        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            GenerateAssetsFolder();
            
            try
            {
                PipeManager = new PipeManager(this);
            }
            catch (Exception e)
            {
                Log.Error($"[Pipe] {e}");
            }
            StyleManager = new StyleManager(BaseDir);

            if (Config.EnabledEvents.MessageEvent || Config.EnabledEvents.MessageWithBitsEvent)
            {
                if (File.Exists(StyleManager.NotificationStyles.MessageNotification.ImagePath) ||
                    File.Exists(StyleManager.NotificationStyles.MessageWithBitsNotification.ImagePath))
                {
                    EventManager.ChatMessageEvent += MessageEventHandler.OnMessageEvent;
                }
                else
                {
                    Log.Warn(
                        "[Pipe] The specified background image for `Message` and `MessageWithBits` notifications doesn't exist.");
                }
            }

            if (Config.EnabledEvents.FollowEvent)
            {
                if (File.Exists(StyleManager.NotificationStyles.FollowNotification.ImagePath))
                {
                    EventManager.FollowEvent += FollowEventHandler.OnFollowEvent;
                }
                else
                {
                    Log.Warn("[Pipe] The specified background image for `Follow` notification doesn't exist.");
                }
            }

            if (Config.EnabledEvents.ReSubscriptionEvent)
            {
                if (File.Exists(StyleManager.NotificationStyles.ReSubscription.ImagePath))
                {
                    EventManager.ReSubscriptionEvent += SubscriptionEventHandler.OnReSubscriptionEvent;
                }
                else
                {
                    Log.Warn("[Pipe] The specified background image for `ReSubscription` notification doesn't exist.");
                }
            }

            if (Config.EnabledEvents.NewSubscriptionEvent)
            {
                if (File.Exists(StyleManager.NotificationStyles.NewSubscription.ImagePath))
                {
                    EventManager.NewSubscriptionEvent += SubscriptionEventHandler.OnNewSubscriptionEvent;
                }
                else
                {
                    Log.Warn("[Pipe] The specified background image for `NewSubscription` notification doesn't exist.");
                }
            }

            if (Config.EnabledEvents.RaidEvent)
            {
                if (File.Exists(StyleManager.NotificationStyles.RaidNotification.ImagePath))
                {
                    EventManager.RaidNotificationEvent += RaidNotificationEventHandler.OnRaidNotificationEventHandler;
                }
                else
                {
                    Log.Warn("[Pipe] The specified background image for `Raid` notification doesn't exist.");
                }
            }

            if (Config.GameSpecificStyles)
            {
                EventManager.GameChangeEvent += GameChangeEventHandler.OnGameChangeEvent;
            }
        }

        public void GenerateAssetsFolder()
        {
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }
        }
    }
}