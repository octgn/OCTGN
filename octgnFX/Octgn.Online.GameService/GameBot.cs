/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;
using agsXMPP;
using agsXMPP.Factory;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using log4net;
using Octgn.Site.Api;
using Skylabs.Lobby;
using LoginResult = Octgn.Site.Api.LoginResult;

namespace Octgn.Online.GameService
{
    using System.Runtime.Caching;
    using System.Threading;
    using System.Timers;

    using Timer = System.Timers.Timer;

    public class GameBot : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static GameBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static GameBot Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameBotSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private GameBot()
        {
            refreshGamesTimer.Start();
            refreshGamesTimer.Elapsed += RefreshGamesTimerOnElapsed;
        }

        private void RefreshGamesTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                refreshGamesTimer.Enabled = false;

                if (userRequests.GetCount() == 0) return;
                var games = GameManager.Instance.Games.ToArray();

                foreach (var game in games)
                {
                    var strname = "hostrequest_" + game.Id;
                    if (userRequests.Contains(strname))
                    {
                        userRequests.Remove(strname);

                        SendGameReady(game);
                    }
                }

            }
            finally
            {
                refreshGamesTimer.Enabled = true;
            }
        }

        private readonly MemoryCache userRequests = new MemoryCache("userRequests");

        private readonly Timer refreshGamesTimer = new Timer(2000);

        private XmppClientConnection Xmpp { get; set; }

        public ObjectHandler OnCheckRecieved { get; set; }

        public void Start()
        {
            RemakeXmpp();
        }

        public void RemakeXmpp()
        {
            if (Xmpp != null)
            {
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                Xmpp.OnMessage -= XmppOnOnMessage;
                Xmpp.OnError -= XmppOnOnError;
                Xmpp.OnAuthError -= Xmpp_OnAuthError;
                Xmpp.OnStreamError -= XmppOnOnStreamError;
                Xmpp.OnAgentStart -= XmppOnOnAgentStart;
                Xmpp.Close();
                Xmpp = null;
            }
            Xmpp = new XmppClientConnection(AppConfig.Instance.ServerPath);
            ElementFactory.AddElementType("hostgamerequest", "octgn:hostgamerequest", typeof(HostGameRequest));

            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.Username = AppConfig.Instance.XmppUsername;
            Xmpp.Password = AppConfig.Instance.XmppPassword;
            Xmpp.Priority = 1;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnAuthError += Xmpp_OnAuthError;
            Xmpp.OnStreamError += XmppOnOnStreamError;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
        }

        public void SendGameReady(HostedGameData game)
        {
            var m = new Message(game.Username + "@of.octgn.net", MessageType.normal, "", "gameready");
            m.GenerateId();
            m.AddChild(game);
            Xmpp.Send(m);
        }

        private void XmppOnOnStreamError(object sender, Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
                Log.Error("Someone Logged In As GameBot");
        }

        private void XmppOnOnAgentStart(object sender)
        {
            if (OnCheckRecieved != null)
                OnCheckRecieved.Invoke(sender);
        }

        public void CheckBotStatus()
        {
            Xmpp.RequestAgents();
        }

        void Xmpp_OnAuthError(object sender, Element e)
        {
            Log.ErrorFormat("AuthError: {0}", e);
        }

        private void XmppOnOnError(object sender, Exception exception)
        {
            Log.Error("[Bot]Error", exception);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            Log.InfoFormat("[Bot]Connection State Changed To {0}", state);
            if (state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageType.normal:
                        if (msg.Subject == "hostgame")
                        {

                            if (msg.HasChildElements == false)
                            {
                                // F it, someone screwed up this year.
                                return;
                            }

                            if (msg.ChildNodes.OfType<HostGameRequest>().Any() == false)
                            {
                                // Again, what the fuuuuu
                                return;
                            }

                            var req = msg.ChildNodes.OfType<HostGameRequest>().First();

                            Log.InfoFormat("Host game from {0}", msg.From);
                            while (SasUpdater.Instance.IsUpdating)
                            {
                                Thread.Sleep(100);
                            }
                            var id = GameManager.Instance.HostGame(req, new User(msg.From));

                            if (id != Guid.Empty)
                                userRequests.Add("hostrequest_" + id, id, DateTimeOffset.UtcNow.AddSeconds(30));
                        }
                        else if (msg.Subject == "gamelist")
                        {
                            // If someone tried to refresh their game list too soon, f them
                            if (userRequests.Contains("refreshrequest_" + msg.From.User.ToLower()))
                                return;
                            // Mark the user as already requested a list for the next 15 seconds
                            userRequests.Add("refreshrequest_" + msg.From.User.ToLower(), 1, DateTimeOffset.UtcNow.AddSeconds(15));
                            var list = GameManager.Instance.Games;
                            var m = new Message(msg.From, MessageType.normal, "", "gamelist");
                            m.GenerateId();
                            foreach (var a in list)
                            {
                                m.AddChild(a);
                            }
                            Xmpp.Send(m);
                        }
                        else if (msg.Subject == "killgame")
                        {
                            var items = msg.Body.Split(new[] { "#:999:#" }, StringSplitOptions.RemoveEmptyEntries);
                            if (items.Length != 2) return;
                            var client = new ApiClient();
                            var res = client.Login(msg.From.User, items[1]);
                            if (res == LoginResult.Ok)
                            {
                                var id = Guid.Parse(items[0]);
                                GameManager.Instance.KillGame(id);
                            }
                            throw new Exception("Error verifying user " + res);
                        }
                        break;
                    case MessageType.error:
                        break;
                    case MessageType.chat:
                        if (!msg.From.User.Equals("d0c", StringComparison.InvariantCultureIgnoreCase)) return;
                        // Keep this around in case we want to add commands at some point, we'll have an idea on how to write the code
                        //if (msg.Body.Equals("pause"))
                        //{
                        //    _isPaused = true;
                        //    Log.Warn(":::::: PAUSED ::::::");
                        //    var m = new Message(msg.From, MessageType.chat, "Paused");
                        //    m.GenerateId();
                        //    Xmpp.Send(m);
                        //}
                        break;
                }

            }
            catch (Exception e)
            {
                Log.Error("[Bot]XmppOnOnMessage Error", e);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Xmpp != null)
            {
                Xmpp.OnMessage -= XmppOnOnMessage;
                Xmpp.OnError -= XmppOnOnError;
                Xmpp.OnAuthError -= Xmpp_OnAuthError;
                Xmpp.OnStreamError -= XmppOnOnStreamError;
                Xmpp.OnAgentStart -= XmppOnOnAgentStart;
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                try { Xmpp.Close(); }
                catch { }
            }
            userRequests.Dispose();
            refreshGamesTimer.Elapsed -= RefreshGamesTimerOnElapsed;
        }

        #endregion
    }
}
