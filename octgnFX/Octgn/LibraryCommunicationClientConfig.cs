/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Threading;
using System.Threading.Tasks;
using Octgn.Communication;
using Octgn.Communication.Packets;
using Octgn.Communication.Serializers;
using Octgn.Library.Communication;

namespace Octgn
{
    public class LibraryCommunicationClientConfig : IClientConfig, IHandshaker
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

        private readonly ISerializer _serializer = new JsonSerializer();

        public IConnection Create(string host)
        {
            var connection = new TcpConnection(host, _serializer, this);
            //client.InitializeSubscriptionModule();
            //client.InitializeHosting(octgnVersion);
            //client.InitializeStatsModule();
            return connection;
        }

        public Task<HandshakeResult> Handshake(IConnection connection, CancellationToken cancellationToken)
        {
            var req = new Octgn.Communication.Packets.HandshakeRequestPacket("session");
            //var req = new Octgn.Communication.Packets.AuthenticationRequestPacket("session");
            req["sessionKey"] = SessionKey;
            req["userId"] = UserId;
            req["deviceId"] = DeviceId;

            var result = await client.Request(req, cancellationToken);

            return result.As<AuthenticationResult>();

            throw new System.NotImplementedException();
        }

        public Task<HandshakeResult> OnHandshakeRequest(HandshakeRequestPacket request, IConnection connection, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}