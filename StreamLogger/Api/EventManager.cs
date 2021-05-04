using System;
using StreamLogger.Api.EventArgs;

namespace StreamLogger.Api
{
    public static class EventManager
    {
        public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev) where TEventArgs : System.EventArgs;

        public delegate void CustomEventHandler();
        
        public static event CustomEventHandler<ChatMessageEventArgs> ChatMessageEvent;
        public static event CustomEventHandler<ChatMessageWithRewardEventArgs> ChatMessageWithRewardEvent;
        public static event CustomEventHandler<HostNotificationEventArgs> HostNotificationEvent; 
        public static event CustomEventHandler<HostingStartedEventArgs> HostingStartedEvent;
        public static event CustomEventHandler<HostingStoppedEventArgs> HostingStoppedEvent;
        public static event CustomEventHandler<NewSubscriptionEventArgs> NewSubscriptionEvent;
        public static event CustomEventHandler<ReSubscriptionEventArgs> ReSubscriptionEvent;

        public static void OnChatMessageEvent(ChatMessageEventArgs e)
        {
            ChatMessageEvent.InvokeSafely(e);
            // EventHandler<ChatMessageEventArgs> raiseEvent = ChatMessageEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnChatMessageWithRewardEvent(ChatMessageWithRewardEventArgs e)
        {
            ChatMessageWithRewardEvent.InvokeSafely(e);
            // EventHandler<ChatMessageWithRewardEventArgs> raiseEvent = ChatMessageWithRewardEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnHostNotificationEvent(HostNotificationEventArgs e)
        {
            HostNotificationEvent.InvokeSafely(e);
            // EventHandler<HostNotificationEventArgs> raiseEvent = HostNotificationEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnHostingStartedEvent(HostingStartedEventArgs e)
        {
            HostingStartedEvent.InvokeSafely(e);
            // EventHandler<HostingStartedEventArgs> raiseEvent = HostingStartedEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnHostingStoppedEvent(HostingStoppedEventArgs e)
        {
            HostingStoppedEvent.InvokeSafely(e);
            // EventHandler<HostingStoppedEventArgs> raiseEvent = HostingStoppedEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnNewSubscriptionEvent(NewSubscriptionEventArgs e)
        {
            NewSubscriptionEvent.InvokeSafely(e);
            // EventHandler<NewSubscriptionEventArgs> raiseEvent = NewSubscriptionEvent;
            //
            // raiseEvent?.Invoke(null, e);
        }

        public static void OnReSubscriptionEvent(ReSubscriptionEventArgs e)
        {
            ReSubscriptionEvent.InvokeSafely(e);
            // EventHandler<ReSubscriptionEventArgs> raiseEvent = ReSubscriptionEvent;
            //
            // try
            // {
            //     raiseEvent?.Invoke(null, e);
            // }
            // catch (Exception exception)
            // {
            //     Log.Error(exception);
            // }
        }

        public static void InvokeSafely<T>(this CustomEventHandler<T> eventHandler, T args) where T : System.EventArgs
        {
            CustomEventHandler<T> raiseEvent = eventHandler;

            foreach (CustomEventHandler<T> handler in raiseEvent.GetInvocationList())
            {
                try
                {
                    handler(args);
                }
                catch (Exception e)
                {
                    Log.Error($"Error while handling {raiseEvent?.Method.Name} in {handler.Method.ReflectedType?.FullName}: {e}");
                }
            }
        }
    }
}