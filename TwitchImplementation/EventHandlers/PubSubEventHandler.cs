using System;
using StreamLogger;
using TwitchImplementation.TwitchBot.Auth;
using TwitchLib.PubSub.Events;

namespace TwitchImplementation.EventHandlers
{
    public static class PubSubEventHandler
    {
        public static void OnLog(object sender, OnLogArgs e)
        {
            Log.Debug($"[PubSub] {e.Data}");
        }
          
        public static void OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
        {
            Log.Error($"[PubSub] {e.Exception}");
        }
          
        public static void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            Log.Debug($"[PubSub] {e.DisplayName} just redeemed {e.RewardTitle} with message: {e.Message}");
        }
        
        public static void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            Log.Info("[PubSub] Service connected."); //TODO Get this to work properly.
            // if (Main.Instance._bot._anonymous)
            // {
            //     Main.Instance._bot._pubsub.SendTopics();
            // }
            // else
            // {
            //     Main.Instance._bot._pubsub.SendTopics(AuthenticationManager.TokenData.AccessToken);
            // }
        }
                
        public static void OnListenResponse(object sender, OnListenResponseArgs e)
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
    }
}