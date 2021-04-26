using System;
using StreamLogger.Api.EventArgs;

namespace StreamLogger.Api
{
    public static class EventManager
    {
        public static event EventHandler<ChatMessageEventArgs> ChatMessageEvent;
        public static event EventHandler<HostNotificationEventArgs> HostNotificationEvent; 
        public static event EventHandler<HostingStartedEventArgs> HostingStartedEvent;
        public static event EventHandler<HostingStoppedEventArgs> HostingStoppedEvent; 

        public static void OnChatMessageEvent(ChatMessageEventArgs e)
        {
            EventHandler<ChatMessageEventArgs> raiseEvent = ChatMessageEvent;

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
    }
}