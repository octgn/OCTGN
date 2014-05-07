/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using log4net;
using Octgn.Library;

namespace Skylabs.Lobby.Messages
{
    public abstract class BaseXmpp : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected XmppClientConnection Xmpp { get; set; }
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private readonly AutoResetEvent _waitForAgents = new AutoResetEvent(false);

        public BaseXmpp(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
        }

        public virtual void Start()
        {
            RemakeXmpp();
        }

        public Task<bool> CheckStatus()
        {
            Xmpp.RequestAgents();
            return Task.Factory.StartNew<bool>(() =>
            {
                return _waitForAgents.WaitOne(TimeSpan.FromSeconds(10));
            });
        }

        public void Reset()
        {
            RemakeXmpp();
        }

        protected abstract void OnResetXmpp();
		
        private void RemakeXmpp()
        {
            if (Xmpp != null)
            {
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                Xmpp.Close();
                Xmpp = null;
            }
            Xmpp = new XmppClientConnection(_url);
            //ElementFactory.AddElementType("hostgamerequest", "octgn:hostgamerequest", typeof(HostGameRequest));

            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.Username = _username;
            Xmpp.Password = _password;
            Xmpp.Priority = 1;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnAuthError += Xmpp_OnAuthError;
            Xmpp.OnStreamError += XmppOnOnStreamError;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
            OnResetXmpp();
        }

        private void XmppOnOnAgentStart(object sender)
        {
            _waitForAgents.Set();
        }

        private void XmppOnOnStreamError(object sender , Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
                Log.Error("Someone replaced this connection");
        }

        void Xmpp_OnAuthError(object sender, Element e)
        {
            Log.ErrorFormat("AuthError: {0}",e);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender , XmppConnectionState state)
        {
            Log.InfoFormat("[Bot]Connection State Changed To {0}",state);
            if(state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private void XmppOnOnError(object sender , Exception exception)
        {
            Log.Error("[Bot]Error", exception);
        }

        public virtual void Dispose()
        {
            if (Xmpp != null)
            {
                Xmpp.OnError -= XmppOnOnError;
                Xmpp.OnAuthError -= Xmpp_OnAuthError;
                Xmpp.OnStreamError -= XmppOnOnStreamError;
                Xmpp.OnAgentStart -= XmppOnOnAgentStart;
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                try { Xmpp.Close(); }
                catch { }
            }
        }
    }

    public class Messanger : BaseXmpp
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<Type, List<Action<object>>> _map = new Dictionary<Type, List<Action<object>>>();
        private readonly ReaderWriterLockSlim _mapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private Action<object> _onLogin;

        public Messanger(string url, string username, string password)
            : base(url, username, password)
        {
        }

        public void OnLogin(Action<object> action)
        {
            _onLogin = action;
        }

        public void Send(GenericMessage message)
        {
            if (Xmpp == null)
                return;
            message.GenerateId();
            message.Write();
            Log.DebugFormat("Sending message {0}", message.Subject);
            Xmpp.Send(message);
            //var mess = new Message(message.To, message.Type, "adegah", message.Subject);
            //mess.GenerateId();
            //Xmpp.Send(mess);
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

        protected override void OnResetXmpp()
        {
            Xmpp.OnMessage += XmppOnMessage;
            Xmpp.OnLogin += XmppOnLogin;
        }

        private void XmppOnLogin(object sender)
        {
            if (_onLogin != null)
                _onLogin(sender);
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

        public override void Dispose()
        {
            Xmpp.OnMessage -= XmppOnMessage;
            if (_map != null)
            {
                foreach (var i in _map)
                {
                    i.Value.Clear();
                }
                _map.Clear();
            }
            base.Dispose();
        }
    }

}