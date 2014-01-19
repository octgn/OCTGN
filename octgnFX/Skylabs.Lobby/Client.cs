// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   The Lobby Client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Skylabs.Lobby
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Timers;

    using agsXMPP;
    using agsXMPP.Factory;
    using agsXMPP.Net;
    using agsXMPP.protocol;
    using agsXMPP.protocol.client;
    using agsXMPP.protocol.iq.agent;
    using agsXMPP.protocol.iq.roster;
    using agsXMPP.protocol.iq.vcard;
    using agsXMPP.protocol.x.muc;
    using agsXMPP.Xml.Dom;

    using log4net;

    using Error = agsXMPP.protocol.Error;


    #region Delegates

    /// <summary>
    /// The delegate register complete.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="results">
    /// The results.
    /// </param>
    public delegate void ClientRegisterComplete(object sender, RegisterResults results);

    /// <summary>
    /// The delegate state changed.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="state">
    /// The state.
    /// </param>
    public delegate void ClientStateChanged(object sender, XmppConnectionState state);

    /// <summary>
    /// The delegate friend request.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="user">
    /// The user.
    /// </param>
    public delegate void ClientFriendRequest(object sender, Jid user);

    /// <summary>
    /// The delegate login complete.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="results">
    /// The results.
    /// </param>
    public delegate void ClientLoginComplete(object sender, LoginResults results);

    /// <summary>
    /// The delegate data received.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="type">
    /// The type.
    /// </param>
    /// <param name="data">
    /// The data.
    /// </param>
    public delegate void ClientDataRecieved(object sender, DataRecType type, object data);

    #endregion

    public class Client
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Events

        /// <summary>
        /// The on register complete.
        /// </summary>
        public event ClientRegisterComplete OnRegisterComplete;

        /// <summary>
        /// The on login complete.
        /// </summary>
        public event ClientLoginComplete OnLoginComplete;

        /// <summary>
        /// The on state changed.
        /// </summary>
        public event ClientStateChanged OnStateChanged;

        /// <summary>
        /// The on friend request.
        /// </summary>
        public event ClientFriendRequest OnFriendRequest;

        /// <summary>
        /// The on data received.
        /// </summary>
        public event ClientDataRecieved OnDataReceived;

        /// <summary>
        /// The on disconnect.
        /// </summary>
        public event EventHandler OnDisconnect;

        #endregion
        #region PrivateAccessors

        /// <summary>
        /// The this.xmpp.
        /// </summary>
        private XmppClientConnection xmpp;

        /// <summary>
        /// The note id.
        /// </summary>
        private int noteId;

        /// <summary>
        /// The my presence.
        /// </summary>
        private Presence myPresence;

        /// <summary>
        /// The games.
        /// </summary>
        private List<HostedGameData> games;

        /// <summary>
        /// The email.
        /// </summary>
        private string email;

        /// <summary>
        /// Logging In
        /// </summary>
        private bool loggingIng;

        private bool isReconnecting;

        private readonly Timer reconnectTimer = new Timer(10000);

        private int reconnectCount = 0;

        #endregion

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        public List<Notification> Notifications { get; set; }

        /// <summary>
        /// Gets or sets the friends.
        /// </summary>
        public List<User> Friends
        {
            get
            {
                lock (friendsLocker) return friends;
            }
            set
            {
                lock (friendsLocker)
                {
                    friends = value;
                }
            }
        }

        private List<User> friends;
        private readonly object friendsLocker = new object();

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets or sets the custom status.
        /// </summary>
        public string CustomStatus
        {
            get
            {
                return this.xmpp.Status;
            }

            set
            {
                this.SetCustomStatus(value);
            }
        }

        /// <summary>
        /// Gets or sets the MUC manager.
        /// </summary>
        public MucManager MucManager { get; set; }

        /// <summary>
        /// Gets the roster manager.
        /// </summary>
        public RosterManager RosterManager
        {
            get
            {
                return this.xmpp.RosterManager;
            }
        }

        /// <summary>
        /// Gets the me.
        /// </summary>
        public User Me { get; private set; }

        /// <summary>
        /// Gets or sets the chatting.
        /// </summary>
        public Chat Chatting { get; set; }

        /// <summary>
        /// Gets or sets the current hosted game port.
        /// </summary>
        public int CurrentHostedGamePort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether disconnected because connection replaced.
        /// </summary>
        public bool DisconnectedBecauseConnectionReplaced { get; set; }

        public bool IsConnected { get; private set; }

        /// <summary>
        /// The host.
        /// </summary>
        public ILobbyConfig Config;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public UserStatus Status
        {
            get
            {
                var s = User.PresenceToStatus(this.myPresence);
                if (s == UserStatus.Unknown)
                {
                    s = this.Me.Status;
                }

                return s;
            }

            set
            {
                this.SetStatus(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="config">Lobby Config</param>
        public Client(ILobbyConfig config)
        {
            Config = config;
            reconnectTimer.Elapsed += ReconnectTimerOnElapsed;
            this.RebuildXmpp();
        }

        private void ReconnectTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (reconnectTimer)
            {
                Log.Info("Reconnect Timer Tick");
                if (this.isReconnecting && !this.IsConnected
                    && this.xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
                {
                    this.RebuildXmpp();
                    this.BeginConnect();
                }
            }
        }

        private void RebuildXmpp()
        {
            if (this.xmpp != null)
            {
                //    this.xmpp.OnXmppConnectionStateChanged -= this.XmppOnOnXmppConnectionStateChanged;
                this.xmpp.Close();
                //    this.xmpp = null;
            }

            this.DisconnectedBecauseConnectionReplaced = false;
            if (this.xmpp == null)
            {
                this.xmpp = new XmppClientConnection(this.Config.ChatHost);

                ElementFactory.AddElementType("gameitem", "octgn:gameitem", typeof(HostedGameData));
                ElementFactory.AddElementType("hostgamerequest", "octgn:hostgamerequest", typeof(HostGameRequest));
                ElementFactory.AddElementType("invitetogamerequest", "octgn:invitetogamerequest", typeof(InviteToGameRequest));
                this.Notifications = new List<Notification>();
                this.Friends = new List<User>();
                this.xmpp.OnRegistered += this.XmppOnOnRegistered;
                this.xmpp.OnRegisterError += this.XmppOnOnRegisterError;
                this.xmpp.OnXmppConnectionStateChanged += this.XmppOnOnXmppConnectionStateChanged;
                this.xmpp.OnLogin += this.XmppOnOnLogin;
                this.xmpp.OnAuthError += this.XmppOnOnAuthError;
                this.xmpp.OnRosterItem += this.XmppOnOnRosterItem;
                this.xmpp.OnRosterEnd += this.XmppOnOnRosterEnd;
                this.xmpp.OnRosterStart += this.XmppOnOnRosterStart;
                this.xmpp.OnMessage += this.XmppOnOnMessage;
                this.xmpp.OnPresence += this.XmppOnPresence;
                this.xmpp.OnAgentItem += this.XmppOnOnAgentItem;
                this.xmpp.OnIq += this.XmppOnOnIq;
                this.xmpp.OnReadXml += this.XmppOnOnReadXml;
                this.xmpp.OnClose += this.XmppOnOnClose;
                this.xmpp.OnWriteXml += this.XmppOnOnWriteXml;
                this.xmpp.OnError += this.XmppOnOnError;
                this.xmpp.OnSocketError += this.XmppOnOnSocketError;
                this.xmpp.OnStreamError += this.XmppOnOnStreamError;
            }
            if (this.Chatting == null)
                this.Chatting = new Chat(this, this.xmpp);
            else
                this.Chatting.Reconnect(this, this.xmpp);
            this.IsConnected = false;
            this.myPresence = new Presence();
            this.CurrentHostedGamePort = -1;
            this.games = new List<HostedGameData>();
        }

        #region XMPP

        /// <summary>
        /// The xmpp on on stream error.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        private void XmppOnOnStreamError(object sender, Element element)
        {
            Log.Warn(element);
            var st = element as Error;
            if (st != null && st.Condition == StreamErrorCondition.Conflict)
            {
                this.DisconnectedBecauseConnectionReplaced = true;
                this.IsConnected = false;
            }

            string textTag = element.GetTag("text");
            if (!string.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
            {
                this.DisconnectedBecauseConnectionReplaced = true;
            }

            Log.Warn("[Xmpp]StreamError: " + element);
        }

        /// <summary>
        /// The xmpp on on socket error.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        private void XmppOnOnSocketError(object sender, Exception exception)
        {
            Log.Warn("Xmpp Socket Error ", exception);
            var se = exception as SocketException;
            if (se != null)
            {
                if (this.loggingIng)
                {
                    this.FireLoginComplete(
                        se.ErrorCode == 10013 ? LoginResults.FirewallError : LoginResults.ConnectionError);
                }
            }
            else if (exception is ConnectTimeoutException)
            {
                if (this.loggingIng)
                {
                    this.FireLoginComplete(LoginResults.ConnectionError);
                }
            }
            else
            {
                Log.Warn("[Xmpp]SocketError: " + exception.Message);
            }
        }

        /// <summary>
        /// The xmpp on on error.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        private void XmppOnOnError(object sender, Exception exception)
        {
            Log.Warn("Xmpp Error ", exception);
            //Trace.WriteLine("[Xmpp]Error: " + exception.Message);
        }

        /// <summary>
        /// The xmpp on on close.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void XmppOnOnClose(object sender)
        {
            Log.Info("Xmpp Closed");
            //Trace.WriteLine("[Xmpp]Closed");
            IsConnected = false;
        }

        /// <summary>
        /// The xmpp on on write xml.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="xml">
        /// The xml.
        /// </param>
        private void XmppOnOnWriteXml(object sender, string xml)
        {
#if(DEBUG)
            Trace.WriteLine("[Xmpp]out: " + xml);
#endif
        }

        /// <summary>
        /// The xmpp on on read xml.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="xml">
        /// The xml.
        /// </param>
        private void XmppOnOnReadXml(object sender, string xml)
        {
#if(DEBUG)
            //Trace.WriteLine("[Xmpp]in: " + xml);
#endif
        }

        /// <summary>
        /// The xmpp on on iq.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="iq">
        /// The iq.
        /// </param>
        private void XmppOnOnIq(object sender, IQ iq)
        {
            if (iq.Error != null && iq.Error.Code == ErrorCode.NotAllowed)
                if (OnLoginComplete != null) OnLoginComplete.Invoke(this, LoginResults.Failure);
            if (iq.Type == IqType.result)
            {
                if (iq.Vcard != null)
                {
                    //var f = this.Friends.AsParallel().FirstOrDefault(x => x.FullUserName == iq.From.Bare);
                    //if (f != null)
                    //{
                    //    var email = DatabaseHandler.GetUser(f.FullUserName);
                    //    if (string.IsNullOrWhiteSpace(email))
                    //    {
                    //        var s =
                    //            iq.Vcard.GetEmailAddresses().FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.UserId));
                    //        if (s != null)
                    //        {
                    //            f.Email = s.UserId;
                    //            DatabaseHandler.AddUser(f.FullUserName, f.Email);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        f.Email = email;
                    //    }
                    //}

                    if (this.OnDataReceived != null)
                    {
                        this.OnDataReceived.Invoke(this, DataRecType.FriendList, this.Friends);
                    }
                }
            }
        }

        /// <summary>
        /// The xmpp on on agent item.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="agent">
        /// The agent.
        /// </param>
        private void XmppOnOnAgentItem(object sender, Agent agent)
        {
        }

        /// <summary>
        /// The xmpp on presence.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="pres">
        /// The pres.
        /// </param>
        private void XmppOnPresence(object sender, Presence pres)
        {
            // Most of this if block handles the status if logged in somewhere else as well.
            if (pres.From.User == this.xmpp.MyJID.User)
            {
                if (pres.Type == PresenceType.subscribe)
                {
                    this.xmpp.PresenceManager.ApproveSubscriptionRequest(pres.From);
                }
                else
                {
                    this.myPresence = pres;
                    this.myPresence.Type = PresenceType.available;
                    if (pres.Show != ShowType.NONE)
                    {
                        this.myPresence.Show = pres.Show;
                    }

                    this.xmpp.Status = this.myPresence.Status ?? this.xmpp.Status;
                    if (this.OnDataReceived != null)
                    {
                        this.OnDataReceived.Invoke(this, DataRecType.MyInfo, pres);
                    }
                }

                return;
            }

            switch (pres.Type)
            {
                case PresenceType.subscribe:
                    this.AcceptFriendship(pres.From.Bare);
                    break;
                case PresenceType.subscribed:
                    break;
                case PresenceType.unsubscribe:
                    break;
                case PresenceType.unsubscribed:
                    break;
                case PresenceType.error:
                    break;
                case PresenceType.probe:
                    break;
            }
            var presFromUser = pres.From.User;
            var friendToSet = this.Friends.FirstOrDefault(x => x.UserName == presFromUser);
            if (friendToSet != null)
            {
                friendToSet.CustomStatus = pres.Status ?? string.Empty;
                friendToSet.SetStatus(pres);
                this.Friends.Remove(friendToSet);
                this.Friends.Add(friendToSet);
            }

            this.XmppOnOnRosterEnd(this);
        }

        /// <summary>
        /// The xmpp on on message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="msg">
        /// The msg.
        /// </param>
        private void XmppOnOnMessage(object sender, Message msg)
        {
            if (msg.Type == MessageType.normal)
            {
                if (msg.Subject == "gameready")
                {
                    Log.Info("Got gameready message");

                    var game = msg.ChildNodes.OfType<HostedGameData>().FirstOrDefault();
                    if (game == null)
                    {
                        Log.Warn("Game message wasn't in the correct format.");
                        return;
                    }

                    this.CurrentHostedGamePort = game.Port;

					if (this.OnDataReceived != null)
						this.OnDataReceived.Invoke(this, DataRecType.HostedGameReady, game);

                }
                else if (msg.Subject == "gamelist")
                {
                    var list = new List<HostedGameData>();
                    foreach (object a in msg.ChildNodes)
                    {
                        var gi = a as HostedGameData;
                        if (gi != null)
                        {
                            list.Add(gi);
                        }
                    }

                    this.games = list;
                    if (this.OnDataReceived != null)
                    {
                        this.OnDataReceived.Invoke(this, DataRecType.GameList, list);
                    }
                }
                else if (msg.Subject == "refresh")
                {
                    Log.Info("Server wants a refresh of game list");
                    if (this.OnDataReceived != null)
                    {
                        Log.Info("Firing server wants a refresh of game list");
                        this.OnDataReceived.Invoke(this, DataRecType.GamesNeedRefresh, null);
                    }
                }
                else if (msg.Subject == "invitetogamerequest")
                {
                    Log.InfoFormat("Received game invite from user {0}", msg.From.User);
                    InviteToGameRequest req = msg.ChildNodes.OfType<InviteToGameRequest>().FirstOrDefault();
                    if (req == null)
                    {
                        Log.WarnFormat("Tried to read invitetogamerequest packet but it was broken...");
                        return;
                    }
                    if (this.OnDataReceived != null)
                    {
                        var sreq = new InviteToGame();
                        sreq.From = new User(msg.From);
                        sreq.SessionId = req.SessionId;
						sreq.Password = req.Password;
                        this.OnDataReceived.Invoke(this, DataRecType.GameInvite, sreq);
                    }
                }
                else if (msg.From.Bare.ToLower() == this.xmpp.MyJID.Server.ToLower())
                {
                    if (msg.Subject == null)
                    {
                        msg.Subject = string.Empty;
                    }

                    if (msg.Body == null)
                    {
                        msg.Body = string.Empty;
                    }

                    var d = new Dictionary<string, string>();
                    d["Message"] = msg.Body;
                    d["Subject"] = msg.Subject;
                    if (this.OnDataReceived != null)
                    {
                        this.OnDataReceived.Invoke(this, DataRecType.Announcement, d);
                    }
                }
            }
        }

        /// <summary>
        /// The xmpp on on roster start.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void XmppOnOnRosterStart(object sender)
        {
            this.Friends.Clear();
        }

        /// <summary>
        /// The xmpp on on roster end.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void XmppOnOnRosterEnd(object sender)
        {
            if (this.OnDataReceived != null)
            {
                this.OnDataReceived.Invoke(this, DataRecType.FriendList, this.Friends);
            }

            if (!this.Chatting.HasGroupRoom(new User(new Jid("lobby@conference." + this.Config.ChatHost))))
            {
            //if (this.Chatting.Rooms.Count(x => x.IsGroupChat && x.GroupUser.FullUserName == "lobby@conference." + this.Config.ChatHost)
            //    == 0)
            //{
                this.xmpp.RosterManager.AddRosterItem(new Jid("lobby@conference." + this.Config.ChatHost));
                this.xmpp.RequestRoster();
            }
        }

        /// <summary>
        /// The xmpp on on roster item.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        private void XmppOnOnRosterItem(object sender, RosterItem item)
        {
            // Friends.Add(item.);
            switch (item.Subscription)
            {
                case SubscriptionType.none:
                    if (item.Jid.Server == "conference." + this.Config.ChatHost)
                    {
                        this.Chatting.GetRoom(new User(item.Jid), true);
                    }

                    break;
                case SubscriptionType.to:
                    if (item.Jid.User == this.Me.UserName)
                    {
                        break;
                    }

                    if (this.Friends.Count(x => x.UserName == item.Jid.User) == 0)
                    {
                        this.Friends.Add(new User(item.Jid));
                    }

                    break;
                case SubscriptionType.from:
                    if (item.Jid.User == this.Me.UserName)
                    {
                        break;
                    }

                    if (this.Friends.Count(x => x.UserName == item.Jid.User) == 0)
                    {
                        this.Friends.Add(new User(item.Jid));
                    }

                    break;
                case SubscriptionType.both:
                    if (item.Jid.User == this.Me.UserName)
                    {
                        break;
                    }

                    if (this.Friends.Count(x => x.UserName == item.Jid.User) == 0)
                    {
                        this.Friends.Add(new User(item.Jid));
                    }

                    break;
                case SubscriptionType.remove:
                    if (this.Friends.Contains(new User(item.Jid)))
                    {
                        this.Friends.Remove(new User(item.Jid));
                    }

                    break;
            }
        }

        /// <summary>
        /// The xmpp on on auth error.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        private void XmppOnOnAuthError(object sender, Element element)
        {
            Log.WarnFormat("Auth error {0}", element);
            this.FireLoginComplete(LoginResults.AuthError);
            Trace.WriteLine("[XMPP]AuthError: Closing...");
            this.IsConnected = false;
            this.xmpp.Close();
        }

        /// <summary>
        /// The xmpp on on login.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void XmppOnOnLogin(object sender)
        {
            Log.Info("Xmpp Login Complete");
            isReconnecting = false;
            reconnectTimer.Stop();
            this.myPresence.Type = PresenceType.available;
            this.myPresence.Show = ShowType.chat;
            this.MucManager = new MucManager(this.xmpp);
            var room = new Jid("lobby@conference." + this.Config.ChatHost);
            this.MucManager.AcceptDefaultConfiguration(room);
            //TODO [NEW UI] Enable this with new UI
            //this.MucManager.JoinRoom(room, this.Username, this.Password, false);
            this.Me = new User(this.xmpp.MyJID);
            this.Me.SetStatus(UserStatus.Online);
            this.xmpp.PresenceManager.Subscribe(this.xmpp.MyJID);
            IsConnected = true;
            Log.Info("Xmpp Login Firing Login Complete");
            this.FireLoginComplete(LoginResults.Success);
        }

        /// <summary>
        /// The xmpp on on xmpp connection state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        private void XmppOnOnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            Log.InfoFormat("Xmpp Connection State Changed To {0}", state);
            Trace.WriteLine("[Xmpp]State: " + state.ToString());
            if (this.OnStateChanged != null)
            {
                this.OnStateChanged.Invoke(this, state);
            }

            if (state == XmppConnectionState.Disconnected)
            {
                if (this.OnDisconnect != null)
                {
                    this.OnDisconnect.Invoke(this, null);
                }
            }
        }

        /// <summary>
        /// The xmpp on on register error.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="element">
        /// The element.
        /// </param>
        private void XmppOnOnRegisterError(object sender, Element element)
        {
            OnRegisterComplete.Invoke(this, RegisterResults.UsernameTaken);
            Trace.WriteLine("[this.xmpp]Register Error...Closing...");
            this.xmpp.Close();
        }

        /// <summary>
        /// The xmpp on on registered.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        private void XmppOnOnRegistered(object sender)
        {
            this.myPresence.Type = PresenceType.available;
            this.myPresence.Show = ShowType.chat;
            this.MucManager = new MucManager(this.xmpp);
            var room = new Jid("lobby@conference." + this.Config.ChatHost);
            this.MucManager.AcceptDefaultConfiguration(room);

            // MucManager.JoinRoom(room,Username,Password,false);
            this.Me = new User(this.xmpp.MyJID);
            this.Me.SetStatus(UserStatus.Online);
            this.xmpp.PresenceManager.Subscribe(this.xmpp.MyJID);

            var v = new Vcard();
            var e = new Email { UserId = this.email, Type = EmailType.INTERNET, Value = this.email };
            v.AddChild(e);
            v.JabberId = new Jid(this.Username + "@" + this.Config.ChatHost);
            var vc = new VcardIq(IqType.set, v);
            vc.To = this.Config.ChatHost;
            vc.GenerateId();
            this.xmpp.Send(vc);
            if (this.OnRegisterComplete != null)
            {
                this.OnRegisterComplete.Invoke(this, RegisterResults.Success);
            }
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public void Send(Element e)
        {
            this.xmpp.Send(e);
        }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        public void Send(string s)
        {
            this.xmpp.Send(s);
        }

        #endregion

        #region Event Callers

        /// <summary>
        /// The fire login complete.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void FireLoginComplete(LoginResults result)
        {
            Log.Info("Firing login complete");
            if (this.OnLoginComplete != null)
            {
                Log.Info("Fired login complete");
                this.OnLoginComplete.Invoke(this, result);
            }
        }

        #endregion

        #region Login Register

        /// <summary>
        /// The begin login.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public void BeginLogin(string username, string password)
        {
            this.isReconnecting = false;
            this.Username = username;
            this.Password = password;
            this.xmpp.RegisterAccount = false;
            this.xmpp.AutoAgents = true;
            this.xmpp.AutoPresence = true;
            this.xmpp.AutoRoster = true;
            this.xmpp.Username = username;
            this.xmpp.Password = password;
            this.xmpp.Priority = 1;
            this.xmpp.SocketConnectionType = SocketConnectionType.Direct;
            this.xmpp.UseSSL = false;
            this.BeginConnect();
        }

        internal void BeginConnect()
        {
            this.loggingIng = true;
            this.xmpp.Open();
        }

        /// <summary>
        /// The begin register.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        public void BeginRegister(string username, string password, string email)
        {
            if (this.xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                this.Username = username;
                this.Password = password;
                this.xmpp.RegisterAccount = true;
                this.xmpp.Username = username;
                this.xmpp.Password = password;
                this.email = email;
                this.xmpp.Open();
            }
        }

        #endregion

        /// <summary>
        /// The begin host game.
        /// </summary>
        /// <param name="game">
        /// The game.
        /// </param>
        /// <param name="gamename">
        /// The gamename.
        /// </param>
        public void BeginHostGame(Octgn.DataNew.Entities.Game game, string gamename, string password, string actualgamename, Version sasVersion)
        {
            var hgr = new HostGameRequest(game.Id, game.Version, gamename, actualgamename, password ?? "",sasVersion);
            //string data = string.Format("{0},:,{1},:,{2},:,{3},:,{4}", game.Id.ToString(), game.Version, gamename, password ?? "",actualgamename);
            Log.InfoFormat("BeginHostGame {0}", hgr);
            var m = new Message(this.Config.GameBotUser.JidUser, this.Me.JidUser, MessageType.normal, "", "hostgame");
            m.GenerateId();
            m.AddChild(hgr);
            this.xmpp.Send(m);
        }

        public void KillGame(Guid gameId)
        {
            var m = new Message(this.Config.GameBotUser.JidUser, this.Me.JidUser, MessageType.normal,string.Format("{0}#:999:#{1}",gameId,this.Password) , "killgame");
            m.GenerateId();
            this.xmpp.Send(m);
        }

        public void Disconnect()
        {
            this.xmpp.SocketDisconnect();
        }

        /// <summary>
        /// The begin get game list.
        /// </summary>
        public void BeginGetGameList()
        {
            Log.Info("Begin get game list");
            var m = new Message(this.Config.GameBotUser.JidUser, MessageType.normal, string.Empty, "gamelist");
            m.GenerateId();
            this.xmpp.Send(m);
        }

        /// <summary>
        /// The begin reconnect.
        /// </summary>
        public void BeginReconnect()
        {
            Log.Info("Begin reconnect");
            lock (reconnectTimer)
            {
                if (isReconnecting) return;
                isReconnecting = true;
            }
            reconnectCount = 0;
            reconnectTimer.Interval = 30000;
            reconnectTimer.Start();
            ReconnectTimerOnElapsed(null, null);
        }

        /// <summary>
        /// The accept friendship.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public void AcceptFriendship(Jid user)
        {
            if (Friends.Contains(new User(user)))
                this.Friends.Add(new User(user));
            this.xmpp.PresenceManager.ApproveSubscriptionRequest(user);
            this.xmpp.PresenceManager.Subscribe(user);
            this.xmpp.SendMyPresence();
            if (this.OnDataReceived != null)
            {
                this.OnDataReceived.Invoke(this, DataRecType.FriendList, this.Friends);
            }

            // Xmpp.RequestRoster();
        }

        /// <summary>
        /// The decline friendship.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public void DeclineFriendship(Jid user)
        {
            this.xmpp.PresenceManager.RefuseSubscriptionRequest(user);
        }

        /// <summary>
        /// The get notification list.
        /// </summary>
        /// <returns>
        /// The <see cref="Notification[]"/>.
        /// </returns>
        public Notification[] GetNotificationList()
        {
            return this.Notifications.ToArray();
        }

        /// <summary>
        /// The set custom status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        public void SetCustomStatus(string status)
        {
            this.xmpp.Status = status;
            this.xmpp.SendMyPresence();
        }

        /// <summary>
        /// The set status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        public void SetStatus(UserStatus status)
        {
            Presence p;
            switch (status)
            {
                case UserStatus.Online:
                    p = new Presence(ShowType.NONE, this.xmpp.Status);
                    p.Type = PresenceType.available;
                    this.xmpp.Send(p);
                    this.xmpp.SendMyPresence();
                    break;
                case UserStatus.Away:
                    p = new Presence(ShowType.away, this.xmpp.Status);
                    p.Type = PresenceType.available;
                    this.xmpp.Send(p);
                    this.xmpp.SendMyPresence();
                    break;
                case UserStatus.DoNotDisturb:
                    p = new Presence(ShowType.dnd, this.xmpp.Status);
                    p.Type = PresenceType.available;
                    this.xmpp.Send(p);
                    this.xmpp.SendMyPresence();
                    break;
                case UserStatus.Invisible:
                    p = new Presence(ShowType.NONE, this.xmpp.Status);
                    p.Type = PresenceType.invisible;
                    this.xmpp.Send(p);
                    this.xmpp.SendMyPresence();
                    break;
            }

            this.Me.SetStatus(status);
        }

        /// <summary>
        /// The send friend request.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        public void SendFriendRequest(string username)
        {
            username = username.ToLower();
            if (username == this.Me.UserName.ToLowerInvariant())
            {
                return;
            }

            var j = new Jid(username + "@" + this.Config.ChatHost);

            this.xmpp.RosterManager.AddRosterItem(j);

            this.xmpp.PresenceManager.Subscribe(j);
        }

        /// <summary>
        /// The remove friend.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public void RemoveFriend(User user)
        {
            this.xmpp.PresenceManager.Unsubscribe(user.JidUser);
            this.RosterManager.RemoveRosterItem(user.JidUser);
            this.Friends.Remove(user);
            this.OnDataReceived.Invoke(this, DataRecType.FriendList, this);
        }

        /// <summary>
        /// The get hosted games.
        /// </summary>
        /// <returns>
        /// The <see cref="HostedGameData"/>.
        /// </returns>
        public HostedGameData[] GetHostedGames()
        {
            return this.games.ToArray();
        }

        /// <summary>
        /// The hosted game started.
        /// </summary>
        public void HostedGameStarted()
        {
            var m = new Message(this.Config.GameBotUser.JidUser,
                MessageType.normal, this.CurrentHostedGamePort.ToString(CultureInfo.InvariantCulture), "gamestarted");
            this.xmpp.Send(m);
        }

        /// <summary>
        /// The log out.
        /// </summary>
        public void LogOut()
        {
            Log.Info("Logging out");
            Trace.WriteLine("[Lobby]Log out called.");
            this.Stop();
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            Log.Info("Xmpp Stop called");
            Trace.WriteLine("[Lobby]Stop Called.");
            this.RebuildXmpp();
        }

        public void SendGameInvite(User user, Guid sessionId, string gamePassword)
        {
            Log.InfoFormat("Sending game request to {0}", user.UserName);
            var req = new InviteToGameRequest(sessionId, gamePassword);
            var m = new Message(user.JidUser, this.Me.JidUser, MessageType.normal, "", "invitetogamerequest");
            m.GenerateId();
            m.AddChild(req);
            this.xmpp.Send(m);
        }
    }

    public class InviteToGame
    {
        public User From { get; set; }
        public Guid SessionId { get; set; }
        public string Password { get; set; }
    }
}
