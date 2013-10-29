using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using Octgn.Core.Networking;

namespace Octgn.Server
{
    using log4net.Core;

    public class DeadLog : ILog
    {
        public ILogger Logger { get; private set; }

        public void Debug(object message)
        {
            
        }

        public void Debug(object message, Exception exception)
        {
            
        }

        public void DebugFormat(string format, params object[] args)
        {
            
        }

        public void DebugFormat(string format, object arg0)
        {
            
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            
        }

        public void Info(object message)
        {
        }

        public void Info(object message, Exception exception)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void InfoFormat(string format, object arg0)
        {
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Warn(object message)
        {
        }

        public void Warn(object message, Exception exception)
        {
        }

        public void WarnFormat(string format, params object[] args)
        {
        }

        public void WarnFormat(string format, object arg0)
        {
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Error(object message)
        {
        }

        public void Error(object message, Exception exception)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void ErrorFormat(string format, object arg0)
        {
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void Fatal(object message)
        {
        }

        public void Fatal(object message, Exception exception)
        {
        }

        public void FatalFormat(string format, params object[] args)
        {
        }

        public void FatalFormat(string format, object arg0)
        {
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }
    }
    public class ServerSocket : SocketBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal Server Server;
        internal DateTime LastPingTime;

        public ServerSocket(TcpClient client, Server server) : base(new DeadLog())
        {
            client.Client.SendTimeout = 4000;
            this.Setup(client,new ServerMessageProcessor());
            Server = server;
            LastPingTime = DateTime.Now;
        }

        public void OnPingReceived()
        {
            LastPingTime = DateTime.Now;
        }

        #region Overrides of SocketBase

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
            Log.DebugFormat("ConnectionEvent {0} {1}",this.EndPoint,e);
            switch (e)
            {
                case SocketConnectionEvent.Disconnected:
                    var c = State.Instance.GetClient(this.Client);
                    if(c != null)
                        c.Disconnect();
                    break;
                case SocketConnectionEvent.Connected:
                    break;
                case SocketConnectionEvent.Reconnected:
                    break;
            }
        }

        public override void OnDataReceived(object sender, byte[] data)
        {
            lock (State.Instance.Handler)
                State.Instance.Handler.ReceiveMessage(data.Skip(4).ToArray(), this);
        }

        #endregion
    }

    public class ServerMessageProcessor : SocketMessageProcessorBase
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