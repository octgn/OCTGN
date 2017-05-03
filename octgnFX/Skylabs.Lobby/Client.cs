/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using System.Threading.Tasks;
using Octgn.Chat;

namespace Skylabs.Lobby
{
    public class Client
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Username { get; private set; }
        public string Password { get; private set; }
        public User Me { get; private set; }
        public int CurrentHostedGamePort { get; set; }
        public bool IsConnected => _client.IsConnected;
        public event Octgn.Chat.Communication.Disconnected Disconnected {
            add {
                _client.Disconnected += value;
            }
            remove {
                _client.Disconnected -= value;
            }
        }
        public event Octgn.Chat.Communication.Connected Connected {
            add {
                _client.Connected += value;
            }
            remove {
                _client.Connected -= value;
            }
        }

        private readonly ILobbyConfig _config;
        private readonly Octgn.Chat.Communication.Client _client;

        public Client(ILobbyConfig config)
        {
            _config = config;
            _client = new Octgn.Chat.Communication.Client(new Octgn.Chat.Communication.TcpConnection(_config.ChatHost));
            _client.DeliverableReceived += Client_DeliverableReceived;
        }

        public async Task<Octgn.Chat.Communication.Messages.Login.LoginResultType> Connect(string username, string password)
        {
            Username = username;
            Password = password;
            Me = new User(Username);
            var ret = await _client.Connect(Username, Password);

            if (ret != Octgn.Chat.Communication.Messages.Login.LoginResultType.Ok) {
                Me = null;
            }

            return ret;
        }

        public void Stop()
        {
            Log.Info("Xmpp Stop called");
            Trace.WriteLine("[Lobby]Stop Called.");
            _client.Connection.IsClosed = true;
        }

        public delegate void ClientDataRecieved(object sender, DataRecType type, object data);
        public event ClientDataRecieved OnDataReceived;
        private void Client_DeliverableReceived(object sender, Octgn.Chat.Communication.DeliverableReceivedEventArgs args)
        {
            var deliverable = args.Deliverable;

            if (deliverable is Package) {
                var package = deliverable as Package;
                if (package.Contents is HostedGameData) {
                    Log.Info("Got gameready message");
                    var game = package.Contents as HostedGameData;

                    this.CurrentHostedGamePort = game.Port;
                    this.OnDataReceived?.Invoke(this, DataRecType.HostedGameReady, game);
                }
            }
        }

        public async Task<IDeliverable> Send(IDeliverable deliverable)
        {
            return await _client.Request(deliverable);
        }

        public async Task<HostedGameData> HostGame(Octgn.DataNew.Entities.Game game, string gamename,
            string password, string actualgamename, string gameIconUrl, Version sasVersion, bool specators)
        {
            var hgr = new HostGameRequest(game.Id, game.Version, gamename, actualgamename, gameIconUrl, password ?? "", sasVersion, specators);
            Log.InfoFormat("BeginHostGame {0}", hgr);

            var result = await _client.Request(new Package(_config.GameBotUser.UserName, hgr));
            if (result == null)
                throw new InvalidOperationException("Host game failed. No game data returned.");
            return (result as Package)?.Contents as HostedGameData;

        }

        public void HostedGameStarted()
        {
            var message = new Octgn.Chat.Message(_config.GameBotUser.UserName, "gamestarted");

            _client.Request(message);
        }

        public void SendGameInvite(User user, Guid sessionId, string gamePassword)
        {
            Log.InfoFormat("Sending game request to {0}", user.UserName);
            var req = new InviteToGameRequest(sessionId, gamePassword);

            _client.Request(new Octgn.Chat.Package(user.UserName, req));
        }
    }

    public class InviteToGame
    {
        public User From { get; set; }
        public Guid SessionId { get; set; }
        public string Password { get; set; }
    }
}
