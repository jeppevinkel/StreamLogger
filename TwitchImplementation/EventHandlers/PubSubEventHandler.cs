using System;
using StreamLogger;
using StreamLogger.Api;
using StreamLogger.Api.EventArgs;
using StreamLogger.Api.MessageTypes;
using TwitchImplementation.TwitchBot.Auth;
using TwitchLib.PubSub.Events;

namespace TwitchImplementation.EventHandlers
{
    public class PubSubEventHandler
    {
        private Bot _bot;

        internal PubSubEventHandler(Bot bot)
        {
            _bot = bot;
        }
        
        public void OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"[PubSub] {e.Data}");
        }
          
        public void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Log.Error($"[PubSub] {e.Exception}");
        }
          
        public void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            var reward = new Reward
            {
                Channel = _bot.ChannelDictionary[e.ChannelId],
                ChannelId = e.ChannelId,
                DisplayName = e.DisplayName,
                RewardCost = e.RewardCost,
                RewardId = e.RewardId,
                RewardPrompt = e.RewardPrompt,
                RewardTitle = e.RewardTitle,
                Timestamp = new DateTimeOffset(e.TimeStamp).ToUnixTimeSeconds(),
                Username = e.Login
            };
            
            EventManager.OnRewardEvent(new RewardEventArgs(reward));
        }
        
        public void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            Log.Info("[PubSub] Service connected."); //TODO Get this to work properly.
            try
            {
                if (_bot._anonymous)
                {
                    _bot._pubsub.SendTopics();
                }
                else
                {
                    _bot._pubsub.SendTopics(AuthenticationManager.TokenData.AccessToken);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
            
        }
                
        public void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
            {
                Log.Error($"[PubSub] Failed to listen to {e.Topic}! Response: {e.Response}");
            }
            else
            {
                Log.Info($"[PubSub] Now listening to {e.Topic}");
            }
        }

        public void OnFollow(object sender, OnFollowArgs e)
        {
            EventManager.OnFollowEvent(new FollowEventArgs(new Follow(_bot.ChannelDictionary[e.FollowedChannelId], e.FollowedChannelId, e.DisplayName, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), e.Username, e.UserId)));
        }
    }
}