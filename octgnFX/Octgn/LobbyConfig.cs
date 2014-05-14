namespace Octgn
{
    using Skylabs.Lobby;
	using Octgn.Library;

    using agsXMPP;

    public class LobbyConfig:ILobbyConfig
    {
        #region Singleton

        internal static LobbyConfig SingletonContext { get; set; }

        private static readonly object LobbyConfigSingletonLocker = new object();

        public static LobbyConfig Get()
        {
            lock (LobbyConfigSingletonLocker) return SingletonContext ?? (SingletonContext = new LobbyConfig());
        }

        internal LobbyConfig()
        {
        }

        #endregion Singleton
        
        public string GameBotUsername { get { return this.GetGameBotUsername(); } }

        public string ChatHost { get { return this.GetChatHost(); } }

        public User GameBotUser { get { return this.GetGameBotUser(); } }

        public User MatchamkingBotUser { get { return this.GetMatchamkingBotUser(); } }

        private User GetGameBotUser()
        {
            return new User(new Jid(this.GameBotUsername,this.ChatHost,""));
        }

        internal string GetChatHost()
        {
            return "of.octgn.net";
        }

        internal string GetGameBotUsername()
        {
            if (X.Instance.Debug || X.Instance.ReleaseTest)
                return "gameserv-test";
            return "gameserv";
        }

        internal User GetMatchamkingBotUser()
        {
            return new User(new Jid(GetMatchmakingBotUsername(), this.ChatHost, ""));
        }

        internal string GetMatchmakingBotUsername()
        {
            if (X.Instance.Debug || X.Instance.ReleaseTest)
                return "matchmaking-test";
            return "matchmaking";
        }
    }
}