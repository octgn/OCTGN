﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

using Octgn.Core.Networking;

namespace Octgn.Networking
{
    public class ClientSocket : ReconnectingSocketBase
    {
        internal IServerCalls Rpc { get; set; }
        internal Handler Handler { get; set; }

        public int Muted { get; set; }

        public ClientSocket(IPAddress address, int port)
            : base(0, TimeSpan.Zero, LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType))
        {
            this.Setup(new IPEndPoint(address, port), new ClientMessageProcessor());
            this.Client.Client.SendTimeout = 4000;
            Handler = new Handler();
            Rpc = new BinarySenderStub(this);
        }

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
            base.OnConnectionEvent(sender, e);
            switch (e)
            {
                case SocketConnectionEvent.Disconnected:
                    if (Program.GameEngine != null)
                        Program.GameEngine.IsConnected = false;
                    Program.GameMess.Warning("You have been disconnected from server.");
                    break;
                case SocketConnectionEvent.Connected:
                    if (Program.GameEngine != null)
                        Program.GameEngine.IsConnected = true;
                    break;
                case SocketConnectionEvent.Reconnected:
                    if (Program.GameEngine != null)
                    {
                        Program.GameEngine.IsConnected = true;
                        Program.GameEngine.Resume();
                    }
                    Program.GameMess.System("You have reconnected");

                    break;
                default:
                    throw new ArgumentOutOfRangeException("e");
            }
        }

        public override void OnDataReceived(object sender, byte[] data)
        {
            Program.Dispatcher.BeginInvoke(new Action(() =>
            {
                Handler.ReceiveMessage(data.Skip(4).ToArray());
            }));
        }

        public void SeverConnectionAtTheKnee()
        {
            this.Client.Close();
        }

        public void StartPings()
        {
            Log.Debug("StartPings");
            Task.Factory.StartNew(
                () =>
                {
                    while (Status == SocketStatus.Connected)
                    {
                        Rpc.Ping();
                        Thread.Sleep(2000);
                    }
                    Log.Debug("StartPings Done Pinging");
                });
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