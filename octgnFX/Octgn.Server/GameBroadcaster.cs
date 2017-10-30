using System.Diagnostics;
using Octgn.Online.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Timers;
using log4net;

namespace Octgn.Server
{
    public class GameBroadcaster : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsBroadcasting { get; internal set; }

        internal UdpClient Client { get; set; }
        internal Timer SendTimer { get; set; }
        internal int BroadcastPort { get; set; }

        private readonly State _state;

        public GameBroadcaster(State state, int broadcastPort = 21234) {
            this._state = state;
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
                } catch (Exception ex) {
                    Log.Error("Error closing client", ex);
                }
                this.Client = null;
                this.IsBroadcasting = false;
            }
        }

        private void SendTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs) {
            if (!this.IsBroadcasting)
                return;

            var game = new HostedGame(_state.Game, false);

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
                var ip = IPAddress.Broadcast;
                if (game.Source == HostedGameSource.Online)
                    ip = IPAddress.Loopback;
                this.Client.Send(mess.ToArray(), mess.Count, new IPEndPoint(ip, BroadcastPort));
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