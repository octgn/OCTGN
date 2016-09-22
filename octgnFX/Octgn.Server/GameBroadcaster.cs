/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Timers;

using log4net;

using Octgn.Library;
using System.Diagnostics;
using Octgn.Online.Library.Models;

namespace Octgn.Server
{
    public class GameBroadcaster : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsBroadcasting { get; internal set; }

        internal UdpClient Client { get; set; }
        internal Timer SendTimer { get; set; }
        internal int BroadcastPort { get; set; }

        private readonly IHostedGameState _game;
        private readonly IOctgnServerSettings _settings;

        public GameBroadcaster(IHostedGameState game, IOctgnServerSettings settings, int broadcastPort = 21234) {
            this.BroadcastPort = broadcastPort;
            this.IsBroadcasting = false;
            this.SendTimer = new Timer(5000);
            this.SendTimer.Elapsed += this.SendTimerOnElapsed;
            _game = game;
            _settings = settings;
        }

        public void StartBroadcasting() {
            Log.Debug("StartBroadcasting called");
            lock (this) {
                if (this.IsBroadcasting) {
                    Log.Debug("StartBroadcasting already broadcasting");
                    return;
                }
                try {
                    if (this.Client == null) {
                        this.Client = new UdpClient();
                    }
                    this.SendTimer.Start();
                    this.IsBroadcasting = true;
                } catch (Exception e) {
                    Log.Error("Error broadcasting", e);
                }
            }
        }

        public void StopBroadcasting() {
            lock (this) {
                if (!this.IsBroadcasting)
                    return;

                this.SendTimer.Stop();

                try {
                    this.Client.Close();
                } catch { }
                this.Client = null;
                this.IsBroadcasting = false;
            }
        }

        private void SendTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (!this.IsBroadcasting)
                return;

            var hgd = new BroadcasterHostedGameData {
                ProcessId = Process.GetCurrentProcess().Id,
                GameGuid = _game.GameId,
                GameName = _game.GameName,
                GameStatus = _game.Status == Online.Library.Enums.EnumHostedGameStatus.GameStarted
                ? EHostedGame.GameInProgress
                : EHostedGame.StartedHosting,
                GameVersion = _game.GameVersion,
                HasPassword = _game.HasPassword,
                Name = _game.Name,
                Port = _game.HostUri.Port,
                Source = _settings.IsLocalGame ? HostedGameSource.Lan : HostedGameSource.Online,
                TimeStarted = _settings.StartTime,
                Username = _game.HostUserName,
                Id = _game.Id,
                Spectator = _game.Spectators,
                GameIconUrl = _game.GameIconUrl,
                UserIconUrl = _game.HostUserIconUrl
            };

            using (var ms = new MemoryStream()) {
                var ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                ser.Serialize(ms, hgd);
                ms.Flush();

                ms.Position = 0;
                var bytes = ms.ToArray();
                var mess = new List<byte>();
                mess.AddRange(BitConverter.GetBytes((Int32)bytes.Length));
                mess.AddRange(bytes);
                var ip = IPAddress.Broadcast;
                if (hgd.Source == HostedGameSource.Online)
                    ip = IPAddress.Loopback;
                this.Client.Send(mess.ToArray(), mess.Count, new IPEndPoint(ip, BroadcastPort));
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            try {
                StopBroadcasting();
            } catch { }
            SendTimer.Elapsed -= SendTimerOnElapsed;
            SendTimer.Dispose();
        }

        #endregion
    }
}