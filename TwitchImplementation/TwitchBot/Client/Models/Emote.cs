using System.Collections.Generic;

namespace TwitchImplementation.TwitchBot.Client.Models
{
    public class Emote
    {
        public string Id;
        public string Name;
        public string ImageUrl;
        public int StartIndex;
        public int EndIndex;

        public Emote(string id, string name, int startIndex, int endIndex)
        {
            Id = id;
            Name = name;
            ImageUrl = $"https://static-cdn.jtvnw.net/emoticons/v1/{id}/1.0";
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public static IEnumerable<Emote> Extract(string rawEmoteString, string message)
        {
            if (string.IsNullOrEmpty(rawEmoteString) || string.IsNullOrEmpty(message))
            {
                yield break;
            }

            if (rawEmoteString.Contains("/"))
            {
                foreach (string emoteString in rawEmoteString.Split("/"))
                {
                    string id = emoteString.Split(":")[0];
                    if (emoteString.Contains(","))
                    {
                        foreach (string emote in emoteString.Replace($"{id}:", "").Split(","))
                        {
                            yield return GetEmote(emote, id, message);
                        }
                    }
                    else
                    {
                        yield return GetEmote(emoteString, id, message, true);
                    }
                }
            }
            else
            {
                string id = rawEmoteString.Split(":")[0];
                if (rawEmoteString.Contains(","))
                {
                    foreach (string emote in rawEmoteString.Replace($"{id}:", "").Split(","))
                    {
                        yield return GetEmote(emote, id, message);
                    }
                }
                else
                {
                    yield return GetEmote(rawEmoteString, id, message);
                }
            }
        }

        private static Emote GetEmote(string emoteData, string id, string message, bool single = false)
        {
            int startIndex = -1;
            int endIndex = -1;

            if (single)
            {
                startIndex = int.Parse(emoteData.Split(":")[1].Split("-")[0]);
                endIndex = int.Parse(emoteData.Split(":")[1].Split("-")[1]);
            }
            else
            {
                startIndex = int.Parse(emoteData.Split('-')[0]);
                endIndex = int.Parse(emoteData.Split('-')[1]);
            }
            
            string name = message.Substring(startIndex, (endIndex - startIndex) + 1);

            return new Emote(id, name, startIndex, endIndex);
        }
    }
}