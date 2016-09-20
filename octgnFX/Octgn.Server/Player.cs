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
        /// Time Disconnected
        /// </summary>
        internal DateTime TimeDisconnected = DateTime.Now;
        /// <summary>
        /// Stubs to send messages to the player
        /// </summary>
        internal IClientCalls Rpc;

        private Game _game;

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

        #endregion IHostedGamePlayer

        internal Player(Game game)
        {
            _game = game;
        }

        internal void Setup(IClientCalls rpc)
        {
            Rpc = rpc;
        }

        internal void Disconnect(bool report)
        {
            Connected = false;
            Socket.Disconnect();
            Connected = true;
            OnDisconnect(report);
        }

        internal void OnDisconnect(bool report)
        {
            lock (this)
            {
                if (Connected == false)
                    return;
                this.Connected = false;
            }
            this.TimeDisconnected = DateTime.Now;
            if (this.SaidHello)
                new Broadcaster(State.Instance.Handler).PlayerDisconnect(Id);
            if (report && State.Instance.Engine.IsLocal == false && State.Instance.Handler.GameStarted && this.IsSpectator == false)
            {
                State.Instance.UpdateDcPlayer(this.Nick,true);
            }
        }

        internal void Kick(bool report, string message, params object[] args)
        {
            var mess = string.Format(message, args);
            this.Connected = false;
            this.TimeDisconnected = DateTime.Now;
            var rpc = new BinarySenderStub(this.Socket,State.Instance.Handler);
            rpc.Kick(mess);
            //Socket.Disconnect();
            Disconnect(report);
            if (SaidHello)
            {
                new Broadcaster(_game. State.Instance.Handler)
                    .Error(string.Format(L.D.ServerMessage__PlayerKicked, Nick, mess));
            }
            SaidHello = false;
        }
    }
}