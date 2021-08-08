using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using StreamLogger;
using StreamLogger.Api;
using YouTubeImplementation.EventHandlers;

namespace YouTubeImplementation
{
    public class Main : Implementation<Config>
    {
        private static Main singleton = new Main();

        private Main()
        {
        }
        public static Main Instance => singleton;

        public YouTubeChatClient YtClient;
        public string BaseDir { get; private set; }

        /// <inheritdoc/>
        public override async void Init()
        {
            base.Init();
            
            BaseDir = Path.Combine(Paths.Implementations, Name);

            YtClient = new YouTubeChatClient();
            await YtClient.ConnectAsync();

            // var secrets = new ClientSecrets {ClientId = Config.ClientId, ClientSecret = Config.ClientSecret};
            // UserCredential credential;
            // credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //     secrets,
            //     new[] { YouTubeService.Scope.YoutubeReadonly },
            //     "user", CancellationToken.None, new FileDataStore("YouTube.StreamLogger")).Result;
            //
            // var service = new YouTubeService(new BaseClientService.Initializer()
            // {
            //     HttpClientInitializer =  credential,
            //     ApplicationName = "StreamLogger"
            // });
            //
            // ChannelListResponse channels = service.Channels.List("snippet").Configure(request => request.Mine = true)
            //     .ExecuteAsync()
            //     .Result;
            // if (channels.Items.Count > 0)
            // {
            //     channelName = channels.Items[0].Snippet.Title;
            // }
            //
            // LiveBroadcastListResponse streams = service.LiveBroadcasts.List("snippet,status").Configure(request => request.Mine = true).ExecuteAsync().Result;
            //
            // foreach (LiveBroadcast broadcast in streams.Items)
            // {
            //     Log.Debug($"{broadcast.Snippet.Title} | {broadcast.Id} | {broadcast.Status.LifeCycleStatus}");
            // }
            //
            // if (streams.Items.Count > 0)
            // {
            //     var source = new CancellationTokenSource();
            //     var token = source.Token;
            //     Task chatTask = Task.Factory.StartNew(
            //         () =>
            //         {
            //             var pollingInterval = 2000;
            //             // var chatPageToken = "";
            //             LiveBroadcast broadcast = streams.Items[0];
            //             LiveChatMessagesResource.ListRequest chatMessagesRequest = service.LiveChatMessages
            //                 .List(broadcast.Snippet.LiveChatId, "snippet,authorDetails");
            //             LiveChatMessageListResponse chatMessages = chatMessagesRequest.ExecuteAsync().Result;
            //             chatMessagesRequest.PageToken = chatMessages.NextPageToken;
            //             
            //             while (true)
            //             {
            //                 if (token.WaitHandle.WaitOne(pollingInterval))
            //                     break;
            //
            //                 chatMessages = chatMessagesRequest.ExecuteAsync().Result;
            //                 chatMessagesRequest.PageToken = chatMessages.NextPageToken;
            //                 pollingInterval = (int)(chatMessages.PollingIntervalMillis ?? 1000);
            //                 Log.Debug($"pollingInterval: {pollingInterval}");
            //
            //                 foreach (LiveChatMessage chatMessage in chatMessages.Items)
            //                 {
            //                     ChatEventHandler.OnChatMessage(chatMessage);
            //                 }
            //             }
            //         }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            // }
        }
    }
}