using System;
using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using StreamLogger.Api.MessageTypes.MiscData;
using StreamLogger.Loader;

namespace YouTubeImplementation.EventHandlers
{
    public static class ChatEventHandler
    {
        public static string[] DefaultColors =
        {
            "#1E90FF",
            "#FF69B4"
        };

        public static void OnChatMessage(LiveChatMessage message)
        {
            try
            {
                DateTimeOffset dto = (message.Snippet.PublishedAt is not null ? new DateTimeOffset(message.Snippet.PublishedAt.Value) : DateTimeOffset.Now);
                
                HashSet<string> flags = new HashSet<string>();
                
                string colorHex = DefaultColors[IntegrationLoader.Random.Next(DefaultColors.Length)];
                
                string avatarUrl = message.AuthorDetails.ProfileImageUrl;
                var donationAmount = 0;

                if (message.Snippet.SuperChatDetails is not null)
                {
                    flags.Add($"CURRENCY:{message.Snippet.SuperChatDetails.Currency}");
                    if (message.Snippet.SuperChatDetails.AmountMicros != null)
                        donationAmount = (int) (message.Snippet.SuperChatDetails.AmountMicros.Value / 1000);
                }
                
                ChatMessage chatMsg = new ChatMessage(
                    null,
                    colorHex,
                    message.AuthorDetails.DisplayName,
                    null,
                    flags,
                    message.AuthorDetails.IsChatModerator ?? false,
                    message.AuthorDetails.IsChatSponsor ?? false,
                    message.AuthorDetails.IsChatOwner ?? false,
                    dto.ToUnixTimeSeconds(),
                    message.Snippet.Type,
                    message.AuthorDetails.ChannelId,
                    message.AuthorDetails.ChannelId,
                    Main.Instance.YtClient?.ChannelName,
                    donationAmount,
                    avatarUrl,
                    message.Snippet.DisplayMessage);
                
                ChatMessageEventArgs messageEventArgs = new ChatMessageEventArgs(chatMsg);
                EventManager.OnChatMessageEvent(messageEventArgs);
            }
            catch (Exception exception)
            {
                Log.Error($"[YouTube] {exception}");
            }
        }
    }
}