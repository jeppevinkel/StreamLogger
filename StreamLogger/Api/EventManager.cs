using System;
using StreamLogger.Api.EventArgs;

namespace StreamLogger.Api
{
    public static class EventManager
    {
        public static event EventHandler<ChatMessageEventArgs> RaiseChatMessageEvent;

        public static void OnRaiseChatMessageEvent(ChatMessageEventArgs e)
        {
            EventHandler<ChatMessageEventArgs> raiseEvent = RaiseChatMessageEvent;

            raiseEvent?.Invoke(null, e);
        }
    }
}