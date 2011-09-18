using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Skylabs.Net
{
    public enum DisconnectReason { PingTimeout, RemoteHostDropped, CleanDisconnect };
}

namespace Skylabs.Net.Sockets
{
    public class StateObject
    {
        public TcpClient workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    }

    public abstract class SkySocket
    {
        public TcpClient Sock { get; private set; }

        public bool Connected { get; private set; }

        private List<byte> Buffer= new List<byte>();

        private Thread thread;

        private DateTime LastPingReceived;

        public SkySocket()
        {
            Connected = false;
            Sock = null;
            Buffer = new List<Byte>();
            thread = new Thread(new ThreadStart(run));
        }

        public SkySocket(TcpClient client)
        {
            _Connect(client);
        }

        public bool Connect(string host, int port)
        {
            if(!Connected)
            {
                TcpClient c = new TcpClient();
                try
                {
                    c.Connect(host, port);
                    _Connect(c);
                    return true;
                }
                catch(Exception e)
                {
                    return false;
                }
            }
            return false;
        }

        private void _Connect(TcpClient c)
        {
            Connected = true;
            Sock = c;
            Recieve();
            Buffer = new List<byte>();
            thread = new Thread(new ThreadStart(run));
            LastPingReceived = DateTime.Now;
            thread.Start();
        }

        public abstract void OnMessageReceived(SocketMessage sm);

        public abstract void OnDisconnect(DisconnectReason reason);

        private void run()
        {
            while(Connected)
            {
                DateTime temp = LastPingReceived.AddSeconds(30);
                if(DateTime.Now >= temp)
                {
                    this.Close(DisconnectReason.PingTimeout);
                }
                SocketMessage sm = new SocketMessage("ping");
                Thread.Sleep(10000);
            }
        }

        private void Recieve()
        {
            StateObject state = new StateObject();
            state.workSocket = Sock;
            Sock.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
        }

        public void Close(DisconnectReason reason)
        {
            this.Sock.Client.BeginDisconnect(false, new System.AsyncCallback(delegate(System.IAsyncResult res)
            {
                Connected = false;
                OnDisconnect(reason);
            }), Sock.Client);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                TcpClient client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.Client.EndReceive(ar);

                if(bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    for(int i=0; i < bytesRead; i++)
                        Buffer.Add(state.buffer[i]);
                    HandleInput();
                    // Try and grab more data
                    Recieve();
                }
                else
                {
                    // The network input stream is closed
                    // Handle any remaining data
                    HandleInput();
                }
            }
            catch(Exception e)
            {
                //Skylabs.ConsoleHelper.ConsoleWriter.writeLine(e.ToString(), false);
            }
        }

        private void HandleInput()
        {
            if(Buffer.Count > 8)
            {
                byte[] mlength = new byte[8];
                Buffer.CopyTo(0, mlength, 0, 8);
                long count = BitConverter.ToInt64(mlength, 0);
                if(Buffer.Count >= count + 8)
                {
                    byte[] mdata = new byte[count];
                    Buffer.CopyTo(8, mdata, 0, (int)count);
                    SocketMessage sm = SocketMessage.Deserialize(mdata);

                    Buffer.RemoveRange(0, (int)count + 8);
                    if(sm.Header.ToLower() == "ping")
                    {
                        LastPingReceived = DateTime.Now;
                    }
                    else
                        OnMessageReceived(sm);
                }
            }
        }

        public void WriteMessage(SocketMessage message)
        {
            byte[] data = SocketMessage.Serialize(message);
            byte[] messagesize = BitConverter.GetBytes(data.LongLength);
            try
            {
                Sock.Client.Send(messagesize);
                Sock.Client.Send(data);
            }
            catch(SocketException se)
            {
#if(!DEBUG)
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#else
                //ConsoleEventLog.addEvent(new ConsoleEventError("in WriteMessage", se), true);
#endif
                Close(DisconnectReason.RemoteHostDropped);
            }
        }
    }
}