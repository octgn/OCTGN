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
    public class Player : IHostedGamePlayer
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Stubs to send messages to the player
        /// </summary>
        internal IClientCalls Rpc { get; private set; }

        public bool Connected { get; internal set; }

        private readonly Game _game;
        private readonly IClientCalls _broadcaster;
        private readonly IOctgnServerSettings _settings;

        #region IHostedGamePlayer

        public ulong Id { get; set; }

        public string Name { get; set; }

        public ulong PublicKey { get; set; }

        public EnumPlayerState State { get; set; }

        public ConnectionState ConnectionState { get; set; }

        public bool IsMod { get; set; }

        public bool Kicked { get; set; }

        public bool InvertedTable { get; set; }

        public bool SaidHello { get; set; }

        public bool Disconnected { get; set; }

        public DateTime DisconnectedDate { get; set; }

        #endregion IHostedGamePlayer

        internal Player(Game game, IHostedGamePlayer player, IClientCalls broadcaster, IOctgnServerSettings settings)
        {
            _game = game;
            _broadcaster = broadcaster;
            _settings = settings;
            if (player != null) {
                ConnectionState = player.ConnectionState;
                Id = player.Id;
                InvertedTable = player.InvertedTable;
                IsMod = player.IsMod;
                Kicked = player.Kicked;
                Name = player.Name;
                PublicKey = player.PublicKey;
                SaidHello = player.SaidHello;
                State = player.State;
            }
        }

        internal void Setup(string name, ulong pkey, IClientCalls rpc, bool spectator)
        {
            Rpc = rpc;
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
                _broadcaster.PlayerDisconnect(Id);
            if (report && !_settings.IsLocalGame && _game.Status == EnumHostedGameStatus.GameStarted && this.State != EnumPlayerState.Spectating)
            {
                this.Disconnected = true;
            }
        }

        internal void Kick(bool report, string message, params object[] args)
        {
            var mess = string.Format(message, args);
            this.Connected = false;
            this.DisconnectedDate = DateTime.Now;
            Rpc.Kick(mess);
            Disconnect(report);
            if (SaidHello)
                _broadcaster.Error(string.Format(L.D.ServerMessage__PlayerKicked, Name, mess));
            SaidHello = false;
        }
    }
}