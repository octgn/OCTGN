using Octgn.Library.Communication;

namespace Octgn
{
    public class LibraryCommunicationClientConfig : IClientConfig
    {
        #region Singleton

        internal static LibraryCommunicationClientConfig SingletonContext { get; set; }

        private static readonly object LobbyConfigSingletonLocker = new object();

        public static LibraryCommunicationClientConfig Get()
        {
            lock (LobbyConfigSingletonLocker) return SingletonContext ?? (SingletonContext = new LibraryCommunicationClientConfig());
        }

        internal LibraryCommunicationClientConfig()
        {
        }

        #endregion Singleton

        public string GameBotUsername { get { return this.GetGameBotUsername(); } }

        public string ChatHost { get { return this.GetChatHost(); } }

        internal string GetChatHost()
        {
            return AppConfig.ChatServerHost;
        }

        internal string GetGameBotUsername()
        {
            //if (X.Instance.Debug || X.Instance.ReleaseTest)
            //    return "gameserv-test";
            return "gameserv";
        }
    }
}