using System;
using StreamLogger.Api;
using System.IO;
using OpenVRNotificationPipeIntegration.EventHandlers;
using StreamLogger;

namespace OpenVRNotificationPipeIntegration
{
    public sealed class Main : Integration<Config>
    {
        private static Main singleton = new Main();

        private Main()
        {
            BaseDir = Path.Combine(Paths.Integrations, Name);
            try
            {
                PipeManager = new PipeManager(this);
            }
            catch (Exception e)
            {
                Log.Error($"[Pipe] {e}");
            }
            StyleManager = new StyleManager(BaseDir);
        }
        public static Main Instance => singleton;
        
        public readonly string BaseDir;
        public readonly StyleManager StyleManager;
        public PipeManager PipeManager;
        
        /// <inheritdoc/>
        public override void Init()
        {
            base.Init();

            GenerateAssetsFolder();

            EventManager.ChatMessageEvent += MessageEventHandler.OnMessageEvent;
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