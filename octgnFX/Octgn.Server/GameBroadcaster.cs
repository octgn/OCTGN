using System.Diagnostics;

namespace Octgn.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Timers;

    using log4net;

    using Octgn.Library;

    public class GameBroadcaster : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsBroadcasting{ get; internal set; }

        internal UdpClient Client { get; set; }
        internal Timer SendTimer { get; set; }
        internal int BroadcastPort { get; set; }

        private readonly State _state;

        public GameBroadcaster(State state, int broadcastPort = 21234)
        {
            this._state = state;
            this.BroadcastPort = broadcastPort;
            this.IsBroadcasting = false;
            this.SendTimer = new Timer(5000);
            this.SendTimer.Elapsed += this.SendTimerOnElapsed;
        }

        public void StartBroadcasting()
        {
            Log.Debug("StartBroadcasting called");
            lock (this)
            {
                if (this.IsBroadcasting)
                {
                    Log.Debug("StartBroadcasting already broadcasting");
                    return;
                }
                try
                {
                    if (this.Client == null)
                    {
                        this.Client = new UdpClient();
                    }
					this.SendTimer.Start();
                    this.IsBroadcasting = true;
                }
                catch (Exception e)
                {
                    Log.Error("Error broadcasting", e);
                }
            }
        }

        public void StopBroadcasting()
        {
            lock (this)
            {
                if (!this.IsBroadcasting)
                    return;

                this.SendTimer.Stop();

                try
                {
                    this.Client.Close();
                }
                catch
                {
                }
                this.Client = null;
                this.IsBroadcasting = false;
            }
        }

        private void SendTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!this.IsBroadcasting)
                return;

            var hgd = new BroadcasterHostedGameData();
            hgd.ProcessId = Process.GetCurrentProcess().Id;
            hgd.GameGuid = _state.Game.GameId;
            hgd.GameName = _state.Game.GameName;
            hgd.GameStatus = _state.Handler.GameStarted
                ? EHostedGame.GameInProgress
                : EHostedGame.StartedHosting;
            hgd.GameVersion = _state.Game.GameVersion;
            hgd.HasPassword = _state.Game.HasPassword;
            hgd.Name = _state.Game.Name;
            hgd.Port = _state.Game.HostUri.Port;
            hgd.Source = _state.IsLocal ? HostedGameSource.Lan : HostedGameSource.Online;
            hgd.TimeStarted = _state.Game.DateCreated;
            hgd.UserId = _state.Game.HostUserId;
            hgd.Id = _state.Game.Id;
            hgd.Spectator = _state.Game.Spectators;
            hgd.GameIconUrl = _state.Game.GameIconUrl;
            hgd.UserIconUrl = _state.Game.HostUserIconUrl;

            using (var ms = new MemoryStream())
            {
                var ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                ser.Serialize(ms,hgd);
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
        public void Dispose()
        {
            try
            {
                this.StopBroadcasting();
            }
            catch{}
            this.SendTimer.Elapsed -= this.SendTimerOnElapsed;
            try
            {
                this.SendTimer.Dispose();
            }
            catch { }
        }

        #endregion
    }
}