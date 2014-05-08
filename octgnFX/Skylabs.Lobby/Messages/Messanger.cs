using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using agsXMPP;
using agsXMPP.protocol.client;
using log4net;
using Octgn.Library;

namespace Skylabs.Lobby.Messages
{
    public class Messanger : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<Type, List<Action<object>>> _map = new Dictionary<Type, List<Action<object>>>();
        private readonly ReaderWriterLockSlim _mapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private XmppClientConnection _client;

        public Messanger()
        {
        }

        public void Send(GenericMessage message)
        {
            if (_client == null)
                return;
            message.GenerateId();
            Log.DebugFormat("Sending message {0}", message.Subject);
            _client.Send(message);
        }

        public void Map<T>(Action<T> action) where T : GenericMessage
        {
            try
            {
                _mapLock.EnterWriteLock();
                if (!_map.ContainsKey(typeof(T)))
                {
                    _map.Add(typeof(T), new List<Action<object>>());
                }
                _map[typeof(T)].Add((x) => action(x as T));

            }
            finally
            {
                _mapLock.ExitWriteLock();
            }
        }

        internal void OnResetXmpp(XmppClientConnection c)
        {
            _client = c;
            _client.OnMessage += XmppOnMessage;
        }

        private void XmppOnMessage(object sender, Message msg)
        {
            try
            {
                _mapLock.EnterReadLock();
                var m = GenericMessage.ReadMessage(msg);
                if (m == null)
                {
                    Log.ErrorFormat("Can't read message\n{0}",msg);
                    return;
                }
                List<Action<object>> handlers = null;
                if (!_map.TryGetValue(m.GetType(), out handlers))
                    return;
                foreach (var h in handlers)
                {
                    var h1 = h;
                    X.Instance.Try(()=>h1(m));
                }
            }
            finally
            {
                _mapLock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            _client.OnMessage -= XmppOnMessage;
            if (_map != null)
            {
                foreach (var i in _map)
                {
                    i.Value.Clear();
                }
                _map.Clear();
            }
        }
    }
}