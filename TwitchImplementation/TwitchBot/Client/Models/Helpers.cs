using System.Collections.Generic;

namespace TwitchImplementation.TwitchBot.Client.Models
{
    public static class Helpers
    {
        public static List<KeyValuePair<string, string>> ParseBadges(string badgesStr)
        {
            var badges = new List<KeyValuePair<string, string>>();

            if (!badgesStr.Contains('/')) return badges;
            if (!badgesStr.Contains(","))
                badges.Add(new KeyValuePair<string, string>(badgesStr.Split('/')[0], badgesStr.Split('/')[1]));
            else
                foreach (string badge in badgesStr.Split(','))
                    badges.Add(new KeyValuePair<string, string>(badge.Split('/')[0], badge.Split('/')[1]));

            return badges;
        }
        
        
    }
}