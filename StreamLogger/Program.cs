using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using StreamLogger.Loader;

namespace StreamLogger
{
    class Program
    {
        //static DiscordIntegration di = new();
        static Task LogWriter;

        static void Main(string[] args)
        {
            LogWriter = Task.Run(Log.WriteLog);
            IntegrationLoader.Run();
            
            // ChatMessage testMessageRando = new ChatMessage();
            // testMessageRando.Channel = "#jeppevinkel";
            // testMessageRando.DisplayName = "RandoDude123";
            // testMessageRando.MessageContent = "This is a test message from a rando!";
            // testMessageRando.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //
            // ChatMessage testMessageMod = new ChatMessage();
            // testMessageMod.Channel = "#jeppevinkel";
            // testMessageMod.DisplayName = "BOLL7708";
            // testMessageMod.MessageContent = "This is a test message from a moderator!";
            // testMessageMod.Mod = true;
            // testMessageMod.Subscriber = true;
            // testMessageMod.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //
            // ChatMessage testMessageBroadcaster = new ChatMessage();
            // testMessageBroadcaster.Channel = "#jeppevinkel";
            // testMessageBroadcaster.DisplayName = "Jeppevinkel";
            // testMessageBroadcaster.MessageContent = "This is a test message from the broadcaster!";
            // testMessageBroadcaster.Mod = true;
            // testMessageBroadcaster.Broadcaster = true;
            // testMessageBroadcaster.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //
            // EventManager.OnRaiseChatMessageEvent(new ChatMessageEventArgs(testMessageRando));
            // EventManager.OnRaiseChatMessageEvent(new ChatMessageEventArgs(testMessageMod));
            // EventManager.OnRaiseChatMessageEvent(new ChatMessageEventArgs(testMessageBroadcaster));

            // Log.Debug(ConfigManager.GetInfo());
            // Log.Debug(ConfigManager.cfg.TestConfig);

            Console.ReadLine();
        }
    }
}