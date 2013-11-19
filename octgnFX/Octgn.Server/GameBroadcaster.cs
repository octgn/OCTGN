﻿namespace Octgn.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        public GameBroadcaster()
        {
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

            var hgd = new HostedGameData();
            hgd.GameGuid = State.Instance.Engine.Game.GameId;
            hgd.GameName = State.Instance.Engine.Game.GameName;
            hgd.GameStatus = State.Instance.Handler.GameStarted
                ? EHostedGame.GameInProgress
                : EHostedGame.StartedHosting;
            hgd.GameVersion = State.Instance.Engine.Game.GameVersion;
            hgd.HasPassword = State.Instance.Engine.Game.HasPassword;
            hgd.Name = State.Instance.Engine.Game.Name;
            hgd.Port = State.Instance.Engine.Game.HostUri.Port;
            hgd.Source = State.Instance.Engine.IsLocal ? HostedGameSource.Lan : HostedGameSource.Online;
            hgd.TimeStarted = State.Instance.StartTime;
            hgd.Username = State.Instance.Engine.Game.HostUserName;
            hgd.Id = State.Instance.Engine.Game.Id;

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
                this.Client.Send(mess.ToArray(), mess.Count, new IPEndPoint(IPAddress.Broadcast, 21234));
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

	[Serializable]
    public class HostedGameData : IHostedGameData
    {
        public Guid GameGuid { get; set; }
        public Version GameVersion { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public string GameName { get; set; }
        public string Username { get; set; }
        public bool HasPassword { get; set; }
        public EHostedGame GameStatus { get; set; }
        public DateTime TimeStarted { get; set; }
        public IPAddress IpAddress { get; set; }
        public HostedGameSource Source { get; set; }
	    public Guid Id { get; set; }
    }
}