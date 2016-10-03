/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using log4net;
using Octgn.Library.Localization;
using Octgn.Online.Library.Enums;
using Octgn.Online.Library.Models;

namespace Octgn.Server
{
    public class Player : IHostedGamePlayer {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Stubs to send messages to the player
        /// </summary>
        internal IServerToClientCalls Rpc { get; private set; }

        public bool Connected { get; internal set; }

        private readonly Game _game;
        private readonly IServerToClientCalls _broadcaster;
        private readonly IOctgnServerSettings _settings;
        private readonly IHostedGamePlayer _player;

        #region IHostedGamePlayer

        public Guid Id {
            get { return _player.Id; }
            set { _player.Id = value; }
        }

        public string Name {
            get { return _player.Name; }
            set { _player.Name = value; }
        }

        public ulong PublicKey {
            get { return _player.PublicKey; }
            set { _player.PublicKey = value; }
        }

        public EnumPlayerState State {
            get { return _player.State; }
            set { _player.State = value; }
        }

        public ConnectionState ConnectionState {
            get { return _player.ConnectionState; }
            set { _player.ConnectionState = value; }
        }

        public bool IsMod {
            get { return _player.IsMod; }
            set { _player.IsMod = value; }
        }

        public bool Kicked {
            get { return _player.Kicked; }
            set { _player.Kicked = value; }
        }

        public bool InvertedTable {
            get { return _player.InvertedTable; }
            set { _player.InvertedTable = value; }
        }

        public bool SaidHello {
            get { return _player.SaidHello; }
            set { _player.SaidHello = value; }
        }

        public bool Disconnected {
            get { return _player.Disconnected; }
            set { _player.Disconnected = value; }
        }

        public DateTime DisconnectedDate {
            get { return _player.DisconnectedDate; }
            set { _player.DisconnectedDate = value; }
        }

        #endregion IHostedGamePlayer

        internal Player(Game game, IHostedGamePlayer player, IServerToClientCalls broadcaster, IOctgnServerSettings settings, IServerToClientCalls rpc)
        {
            _game = game;
            _player = player;
            Rpc = rpc;
            _broadcaster = broadcaster;
            _settings = settings;
        }

        internal void Setup(string name, ulong pkey, bool spectator)
        {
            Name = name;
            PublicKey = pkey;
            if (spectator)
                this.State = EnumPlayerState.Spectating;
        }

        internal void Disconnect(bool report)
        {
            OnDisconnect(report);
        }

        internal void OnDisconnect(bool report)
        {
            lock (this)
            {
                if (!Connected)
                    return;
                this.Connected = false;
            }
            this.DisconnectedDate = DateTime.Now;
            if (this.SaidHello)
                _broadcaster.PlayerDisconnect(Id, Id);
            if (report && !_settings.IsLocalGame && _game.Status == EnumHostedGameStatus.GameStarted && this.State != EnumPlayerState.Spectating)
            {
                this.Disconnected = true;
            }
        }

        internal void Kick(Guid sender, bool report, string message, params object[] args)
        {
            var mess = string.Format(message, args);
            this.Connected = false;
            this.DisconnectedDate = DateTime.Now;
            Rpc.Kick(sender, mess);
            Disconnect(report);
            if (SaidHello)
                _broadcaster.Error(sender, string.Format(L.D.ServerMessage__PlayerKicked, Name, mess));
            SaidHello = false;
        }
    }
}