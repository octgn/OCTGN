﻿using System.Net;
using System.Threading.Tasks;

namespace Octgn.Library.Networking
{
    public interface ISocket
    {
        SocketStatus Status { get; }
        IPEndPoint EndPoint { get; }
		ISocketMessageProcessor MessageProcessor { get; }

        void Setup(IPEndPoint ep, ISocketMessageProcessor processor);
        Task Connect();
        void Disconnect();
        void Send(byte[] data);

        void OnConnectionEvent(object sender, SocketConnectionEvent e);
        void OnDataReceived(object sender, byte[] data);
    }

    public enum SocketConnectionEvent
    {
        Disconnected,
		Connected,
		Reconnected
    }

    public enum SocketStatus
    {
        Disconnected,
		Connecting,
		Connected
    }
}