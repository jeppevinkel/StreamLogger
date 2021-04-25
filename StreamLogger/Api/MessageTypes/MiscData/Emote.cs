namespace StreamLogger.Api.MessageTypes.MiscData
{
    public struct Emote
    {
        public string Id;
        public string Name;
        public string ImageUrl;
        public int Start;
        public int End;

        public Emote(string id, int start, int end)
        {
            Id = id;
            Start = start;
            End = end;
            Name = null;
            ImageUrl = null;
        }

        public Emote(string id, int start, int end, string name)
        {
            Id = id;
            Start = start;
            End = end;
            Name = name;
            ImageUrl = null;
        }

        public Emote(string id, int start, int end, string name, string imageUrl)
        {
            Id = id;
            Start = start;
            End = end;
            Name = name;
            ImageUrl = imageUrl;
        }
    }
}