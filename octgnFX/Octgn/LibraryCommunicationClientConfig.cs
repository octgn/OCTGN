using Octgn.Communication;
using Octgn.Library.Communication;
using System;

namespace Octgn
{
    public class LibraryCommunicationClientConfig : IClientConfig
    {
        public string GameBotUsername { get; }

        public string ChatHost { get; }

        public IConnectionCreator ConnectionCreator { get; }

        public LibraryCommunicationClientConfig(IConnectionCreator connectionCreator) : this(AppConfig.ChatServerHost, "gameserv", connectionCreator) {

        }

        public LibraryCommunicationClientConfig(string comHost, string gameBotUsername, IConnectionCreator connectionCreator) {
            if (string.IsNullOrWhiteSpace(comHost)) throw new ArgumentNullException(nameof(comHost));
            if (string.IsNullOrWhiteSpace(gameBotUsername)) throw new ArgumentNullException(nameof(gameBotUsername));
            if (connectionCreator == null) throw new ArgumentNullException(nameof(connectionCreator));

            GameBotUsername = gameBotUsername;
            ChatHost = comHost;
            ConnectionCreator = connectionCreator;
        }
    }
}