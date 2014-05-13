/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.Xml.Dom;
using log4net;

namespace Skylabs.Lobby.Messages
{
    public class XmppClient : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Messanger Messanger { get; set; }
        public XmppClientConnection Xmpp { get; set; }
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;
        private Action<object> _onLogin;
        private readonly AutoResetEvent _waitForAgents = new AutoResetEvent(false);

        public XmppClient(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;
            Messanger = new Messanger();
        }

        public virtual void Connect()
        {
            RemakeXmpp();
        }

        public void OnLogin(Action<object> action)
        {
            _onLogin = action;
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
            Xmpp.OnLogin += XmppOnLogin;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
            OnResetXmpp();
            Messanger.OnResetXmpp(Xmpp);
        }

        protected virtual void OnResetXmpp()
        {

        }

        private void XmppOnOnAgentStart(object sender)
        {
            _waitForAgents.Set();
        }

        private void XmppOnOnStreamError(object sender, Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
                Log.Error("Someone replaced this connection");
        }

        void Xmpp_OnAuthError(object sender, Element e)
        {
            Log.ErrorFormat("AuthError: {0}", e);
        }

        private void XmppOnLogin(object sender)
        {
            if (_onLogin != null)
                _onLogin(sender);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            Log.InfoFormat("[Bot]Connection State Changed To {0}", state);
            if (state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private void XmppOnOnError(object sender, Exception exception)
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
                Xmpp.OnLogin -= XmppOnLogin;
                try { Xmpp.Close(); }
                catch { }
            }
            if (Messanger != null)
                Messanger.Dispose();
        }
    }
}