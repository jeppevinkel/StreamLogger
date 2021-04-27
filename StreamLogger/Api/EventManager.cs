using System;
using StreamLogger.Api.EventArgs;

namespace StreamLogger.Api
{
    public static class EventManager
    {
        public static event EventHandler<ChatMessageEventArgs> ChatMessageEvent;
        public static event EventHandler<ChatMessageWithRewardEventArgs> ChatMessageWithRewardEvent;
        public static event EventHandler<HostNotificationEventArgs> HostNotificationEvent; 
        public static event EventHandler<HostingStartedEventArgs> HostingStartedEvent;
        public static event EventHandler<HostingStoppedEventArgs> HostingStoppedEvent;
        public static event EventHandler<NewSubscriptionEventArgs> NewSubscriptionEvent;
        public static event EventHandler<ReSubscriptionEventArgs> ReSubscriptionEvent;

        public static void OnChatMessageEvent(ChatMessageEventArgs e)
        {
            EventHandler<ChatMessageEventArgs> raiseEvent = ChatMessageEvent;

            raiseEvent?.Invoke(null, e);
        }

        public static void OnChatMessageWithRewardEvent(ChatMessageWithRewardEventArgs e)
        {
            EventHandler<ChatMessageWithRewardEventArgs> raiseEvent = ChatMessageWithRewardEvent;

            raiseEvent?.Invoke(null, e);
        }

        public static void OnHostNotificationEvent(HostNotificationEventArgs e)
        {
            EventHandler<HostNotificationEventArgs> raiseEvent = HostNotificationEvent;
            
            raiseEvent?.Invoke(null, e);
        }

        public static void OnHostingStartedEvent(HostingStartedEventArgs e)
        {
            EventHandler<HostingStartedEventArgs> raiseEvent = HostingStartedEvent;
            
            raiseEvent?.Invoke(null, e);
        }

        public static void OnHostingStoppedEvent(HostingStoppedEventArgs e)
        {
            EventHandler<HostingStoppedEventArgs> raiseEvent = HostingStoppedEvent;
            
            raiseEvent?.Invoke(null, e);
        }

        public static void OnNewSubscriptionEvent(NewSubscriptionEventArgs e)
        {
            EventHandler<NewSubscriptionEventArgs> raiseEvent = NewSubscriptionEvent;
            
            raiseEvent?.Invoke(null, e);
        }

        public static void OnReSubscriptionEvent(ReSubscriptionEventArgs e)
        {
            EventHandler<ReSubscriptionEventArgs> raiseEvent = ReSubscriptionEvent;
            
            raiseEvent?.Invoke(null, e);
        }
    }
}