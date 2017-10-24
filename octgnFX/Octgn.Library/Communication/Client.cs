/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using System.Threading.Tasks;
using Octgn.Communication;
using Octgn.Communication.Chat;

namespace Octgn.Library.Communication
{
    public class Client
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public User Me { get; private set; }
        public bool IsConnected => _client.IsConnected;
        public event Disconnected Disconnected {
            add {
                _client.Disconnected += value;
            }
            remove {
                _client.Disconnected -= value;
            }
        }
        public event Connected Connected {
            add {
                _client.Connected += value;
            }
            remove {
                _client.Connected -= value;
            }
        }

        private readonly IClientConfig _config;
        private readonly Octgn.Communication.Client _client;

        private readonly IAuthenticator _clientAuthenticator;

        public Client(IClientConfig config)
        {
            _config = config;
            _clientAuthenticator = new ClientAuthenticator();
            _client = new Octgn.Communication.Client(new TcpConnection(_config.ChatHost), new Octgn.Communication.Serializers.JsonSerializer(), _clientAuthenticator);
            _client.InitializeChat();
        }

        public async Task Connect(string sessionKey, string userId, string deviceId)
        {
            var clientAuthenticator = _clientAuthenticator as ClientAuthenticator;
            clientAuthenticator.SessionKey = sessionKey;
            clientAuthenticator.UserId = userId;
            clientAuthenticator.DeviceId = deviceId;

            await _client.Connect();

            Me = new User(userId, true);
        }

        public void Stop()
        {
            Log.Info("Xmpp Stop called");
            Trace.WriteLine("[Lobby]Stop Called.");
            _client.Connection.IsClosed = true;
        }

        public event EventHandler<HostedGameReadyEventArgs> HostedGameReady {
            add => _client.Chat().HostedGameReady += value;
            remove => _client.Chat().HostedGameReady -= value;
        }

        public Task<Octgn.Communication.Chat.HostedGame> HostGame(HostGameRequest request)
        {
            Log.Info($"{request}");

            return _client.Chat().RPC.HostGame(request);
        }

        public Task HostedGameStarted(Guid gameId)
        {
            return _client.Chat().RPC.SignalGameStarted(gameId.ToString());
        }
    }
}
