/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Net;
using System.Threading.Tasks;
using log4net;
using Octgn.Library.Networking;
using Microsoft.AspNet.SignalR.Client;
using Octgn.Server.Signalr;
using Microsoft.AspNet.SignalR;

namespace Octgn.Networking
{
    public class ClientSocket : IDisposable
    {
		internal static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal HubRpc Rpc { get; set; }
        internal Handler Handler { get; set; }

        private readonly HubConnection _connection;
        private readonly IHubProxy _hub;

        public ulong Muted { get; set; }
        public IPEndPoint EndPoint { get; private set; }
        private readonly int _gameId;

        public ClientSocket(IPAddress address, int port, int gameId) {
            _gameId = gameId;
            EndPoint = new IPEndPoint(address, port);
            var path = $"http://{address}:{port}/";
            _connection = new HubConnection(path);
            _connection.Headers["gameid"] = _gameId.ToString();
            _hub = _connection.CreateHubProxy(nameof(GameHub));

            _connection.StateChanged += Connection_StateChanged;
            _connection.Reconnected += Connection_Reconnected;

            Handler = new Handler();
            Rpc = new HubRpc();
        }

        public async Task Connect() => await _connection.Start();

        internal void ForceDisconnect() => _connection.Stop();

        protected void SetupConnection() {
            Handler.InitializeHub(_hub);
            Rpc.InitializeHub(_hub);

            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Groups.Add(_connection.ConnectionId, _gameId.ToString());
        }

        private void Connection_StateChanged(StateChange obj) {
            switch (obj.NewState) {
                case ConnectionState.Reconnecting:
                case ConnectionState.Connecting:
                    break;
                case ConnectionState.Disconnected:
                    if (Program.GameEngine != null)
                        Program.GameEngine.IsConnected = false;
                    Program.GameMess.Warning("You have been disconnected from server.");
                    break;
                case ConnectionState.Connected:
                    if (Program.GameEngine != null)
                        Program.GameEngine.IsConnected = true;
                    SetupConnection();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }

        private void Connection_Reconnected() {
            if (Program.GameEngine != null)
            {
                Program.GameEngine.IsConnected = true;
                Program.GameEngine.Resume();
            }
            Program.GameMess.System("You have reconnected");
        }

        public void SeverConnectionAtTheKnee()
        {
            this._connection.Stop();
        }

        public void Dispose() {
            _connection.StateChanged -= Connection_StateChanged;
            _connection.Reconnected -= Connection_Reconnected;
            _connection.Dispose();
        }
    }

    public class ClientMessageProcessor : SocketMessageProcessorBase
    {
        public override int ProcessBuffer(byte[] data)
        {
            if (data.Length < 4) return 0;
            var length = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            if (data.Length < length) return 0;
            return length;
        }
    }
}