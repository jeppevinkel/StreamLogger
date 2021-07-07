using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using StreamLogger.Api.EventArgs;

namespace StreamLogger.Api
{
    public static class Broadcaster
    {
        private static Dictionary<string, CustomEventHandler<BroadcastEventArgs>> _broadcastTopics = new();
        
        public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev) where TEventArgs : System.EventArgs;
        public delegate void CustomEventHandler();
        
        // public static event CustomEventHandler<BroadcastEventArgs> BroadcastEvent;

        public static void Listen(string topic, CustomEventHandler<BroadcastEventArgs> func)
        {
            if (_broadcastTopics.ContainsKey(topic))
            {
                _broadcastTopics[topic] += func;
            }
            else
            {
                _broadcastTopics.Add(topic, func);
            }
        }

        public static void Publish(string topic, string message, string nonce = null, string returnTopic = null)
        {
            Log.Debug($"[Broadcaster] Publishing to {topic}: {message}");
            if (_broadcastTopics.ContainsKey(topic))
            {
                Log.Debug("[Broadcaster] Someone is subscribed to the topic.");
                _broadcastTopics[topic].InvokeSafely(new BroadcastEventArgs(message, nonce, returnTopic));
            }
        }
        
        private static void InvokeSafely<T>(this CustomEventHandler<T> eventHandler, T args) where T : System.EventArgs
        {
            CustomEventHandler<T> raiseEvent = eventHandler;

            if (raiseEvent is null) return;

            foreach (CustomEventHandler<T> handler in raiseEvent.GetInvocationList())
            {
                try
                {
                    handler(args);
                }
                catch (Exception e)
                {
                    Log.Error($"[Broadcaster] Error while handling {raiseEvent?.Method.Name} in {handler.Method.ReflectedType?.FullName}: {e}");
                }
            }
        }

        public static string GetNonce()
        {
            var byteArray = new byte[20];
            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(byteArray);
            }
            return Convert.ToBase64String(byteArray);
        }
    }
}