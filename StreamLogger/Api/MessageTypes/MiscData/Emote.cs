namespace StreamLogger.Api.MessageTypes.MiscData
{
    public struct Emote
    {
        public string Id;
        public int Start;
        public int End;

        public Emote(string id, int start, int end)
        {
            Id = id;
            Start = start;
            End = end;
        }
    }
}