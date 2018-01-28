﻿using System.Diagnostics;
using Octgn.Online.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Timers;
using log4net;

namespace Octgn.Library.Networking
{
    public class GameBroadcaster : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IPAddress MulticastAddress = IPAddress.Parse("228.8.8.1");

        public bool IsBroadcasting { get; internal set; }

        internal UdpClient Client { get; set; }
        internal Timer SendTimer { get; set; }
        internal int BroadcastPort { get; set; }

        private readonly HostedGame _game;

        public GameBroadcaster(HostedGame game, int broadcastPort = 21234) {
            this._game = game;
            this.BroadcastPort = broadcastPort;
            this.IsBroadcasting = false;
            this.SendTimer = new Timer(5000);
            this.SendTimer.Elapsed += this.SendTimerOnElapsed;
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

                        if (_game.Source == HostedGameSource.Online) {
                            Client.EnableBroadcast = false;
                        } else {
                            Client.EnableBroadcast = true;
                        }

                        Client.JoinMulticastGroup(MulticastAddress);
                    }
                    this.SendTimer.Start();
                    this.IsBroadcasting = true;
                    SendTimerOnElapsed(null, null);
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
                } catch (Exception ex) {
                    Log.Error("Error closing client", ex);
                }
                this.Client = null;
                this.IsBroadcasting = false;
            }
        }

        private void SendTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            try {
                if (!this.IsBroadcasting)
                    return;

                var game = new HostedGame(_game, false);

                game.ProcessId = Process.GetCurrentProcess().Id;

                using (var ms = new MemoryStream()) {
                    var ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    ser.Serialize(ms, game);
                    ms.Flush();

                    ms.Position = 0;
                    var bytes = ms.ToArray();
                    var mess = new List<byte>();
                    mess.AddRange(BitConverter.GetBytes((Int32)bytes.Length));
                    mess.AddRange(bytes);

                    this.Client.Send(mess.ToArray(), mess.Count, new IPEndPoint(MulticastAddress, BroadcastPort));
                }
            } catch (Exception ex) {
                Log.Error($"{nameof(SendTimerOnElapsed)}", ex);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            this.StopBroadcasting();
            this.SendTimer.Elapsed -= this.SendTimerOnElapsed;
            try {
                this.SendTimer.Dispose();
            } catch {
                Log.Error($"{nameof(Dispose)}: {nameof(SendTimer)}.{nameof(SendTimer.Dispose)}");
            }
        }

        #endregion
    }
}