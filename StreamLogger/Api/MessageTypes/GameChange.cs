namespace StreamLogger.Api.MessageTypes
{
    public struct GameChange
    {
        public int GameId;
        public string GameName;
        public string GameDescription;
        public string GameCoverUrl;
        public string GameHeaderUrl;
        public bool GameIsFree;
    }
}