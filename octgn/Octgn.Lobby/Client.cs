using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
	//using Octgn.Data;
using agsXMPP;
using agsXMPP.Factory;
using agsXMPP.Net;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.agent;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.vcard;
using agsXMPP.protocol.x.muc;

namespace Octgn.Lobby
{
    public class Client
    {
        #region Enums
            public enum RegisterResults{ConnectionError,Success,UsernameTaken,UsernameInvalid,PasswordFailure}
            public enum LoginResults{ConnectionError,Success,Failure}
            public enum DataRecType{FriendList,MyInfo,GameList,HostedGameReady,GamesNeedRefresh,Announcement}
            public enum LoginResult{Success,Failure,Banned,WaitingForResponse};
            public enum LobbyMessageType { Standard, Error, Topic };
        #endregion
        #region Delegates
            public delegate void DRegisterComplete(object sender, RegisterResults results);
            public delegate void DStateChanged(object sender, string state);
            public delegate void DFriendRequest(object sender, Jid user);
            public delegate void DLoginComplete(object sender, LoginResults results);
            public delegate void DDataRecieved(object sender, DataRecType type, object data);
        #endregion
        #region Events
            public event DRegisterComplete OnRegisterComplete;
            public event DLoginComplete OnLoginComplete;
            public event DStateChanged OnStateChanged;
            public event DFriendRequest OnFriendRequest;
            public event DDataRecieved OnDataRecieved;
            public event EventHandler OnDisconnect;
        #endregion
        #region PrivateAccessors
            private XmppClientConnection _xmpp;
            private int _noteId;
            private Presence _myPresence;
            private List<HostedGameData> _games;
            private string _email;
        #endregion

        public List<Notification> Notifications { get; set; }
        public List<NewUser> Friends { get; set; }
        //public List<NewUser> GroupChats { get; set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string CustomStatus { get { return _xmpp.Status; }set{SetCustomStatus(value);} }
        public MucManager MucManager { get; set; }
        public RosterManager RosterManager { get { return _xmpp.RosterManager; } }
        public NewUser Me { get; private set; }
        public Chat Chatting { get; set; }
        public int CurrentHostedGamePort { get; set; }
        public bool DisconnectedBecauseConnectionReplaced { get; set; }

        public UserStatus Status
        {
            get { return NewUser.PresenceToStatus(_myPresence); }
            set { SetStatus(value); }
        }

        
        public Client()
        {
            RebuildXmpp();
        }

        private void RebuildXmpp()
        {
            if(_xmpp != null)
            {
                _xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                _xmpp.Close();
                _xmpp = null;
            }
            DisconnectedBecauseConnectionReplaced = false;
            _xmpp = new XmppClientConnection("server.octgn.info");
            _xmpp.OnRegistered += XmppOnOnRegistered;
            _xmpp.OnRegisterError += XmppOnOnRegisterError;
            _xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            _xmpp.OnLogin += XmppOnOnLogin;
            _xmpp.OnAuthError += XmppOnOnAuthError;
            _xmpp.OnRosterItem += XmppOnOnRosterItem;
            _xmpp.OnRosterEnd += XmppOnOnRosterEnd;
            _xmpp.OnRosterStart += XmppOnOnRosterStart;
            _xmpp.OnMessage += XmppOnOnMessage;
            _xmpp.OnPresence += XmppOnOnPresence;
            _xmpp.OnAgentItem += XmppOnOnAgentItem;
            _xmpp.OnIq += XmppOnOnIq;
            _xmpp.OnReadXml += XmppOnOnReadXml;
            _xmpp.OnClose += XmppOnOnClose;
            _xmpp.OnWriteXml += XmppOnOnWriteXml;
            _xmpp.OnError += XmppOnOnError;
            _xmpp.OnSocketError += XmppOnOnSocketError;
            _xmpp.OnStreamError += XmppOnOnStreamError;
            Notifications = new List<Notification>();
            Friends = new List<NewUser>();
            //GroupChats = new List<NewUser>();
            _myPresence = new Presence();
            Chatting = new Chat(this, _xmpp);
            CurrentHostedGamePort = -1;
            _games = new List<HostedGameData>();
            ElementFactory.AddElementType("gameitem", "octgn:gameitem", typeof(HostedGameData));
        }

        #region XMPP


        private void XmppOnOnStreamError(object sender, Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection") DisconnectedBecauseConnectionReplaced = true;
            Trace.WriteLine("[Xmpp]StreamError: " + element);
        }

        private void XmppOnOnSocketError(object sender, Exception exception)
        {
            Trace.WriteLine("[Xmpp]SocketError: " + exception.Message);
        }

        private void XmppOnOnError(object sender, Exception exception)
        {
            Trace.WriteLine("[Xmpp]Error: " + exception.Message);
        }

        private void XmppOnOnClose(object sender)
        {
            Trace.WriteLine("[Xmpp]Closed");
        }

        private void XmppOnOnWriteXml(object sender, string xml)
        {
#if(FDEBUG)
            Trace.WriteLine("[Xmpp]out: " + xml);
#endif
        }

        private void XmppOnOnReadXml(object sender , string xml)
        {
#if(FDEBUG)
            Trace.WriteLine("[Xmpp]in: " + xml);
#endif
        }

        private void XmppOnOnIq(object sender, IQ iq)
        {
            if(iq.Error != null && iq.Error.Code == ErrorCode.NotAllowed)
                if(OnLoginComplete != null)OnLoginComplete.Invoke(this,LoginResults.Failure);
            if(iq.Type == IqType.result)
            {
                if (iq.Vcard != null)
                {
                    var f = Friends.SingleOrDefault(x => x.User.Bare == iq.From.Bare);
                    if(f!= null)
                    {
                        var s = iq.Vcard.GetEmailAddresses().SingleOrDefault(x => !String.IsNullOrWhiteSpace(x.UserId));
                        if(s != null) {
                            f.Email = s.UserId;
                        }
                    }

                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(this,DataRecType.FriendList, Friends);
                }
            }

        }

        private void XmppOnOnAgentItem(object sender, Agent agent)
        {

        }

        private void XmppOnOnPresence(object sender, Presence pres)
        {
            if (pres.From.User == _xmpp.MyJID.User)
            {
                _myPresence = pres;
                _myPresence.Type = PresenceType.available;
                _xmpp.Status = _myPresence.Status ?? _xmpp.Status;
                if(OnDataRecieved != null)
                    OnDataRecieved.Invoke(this,DataRecType.MyInfo, pres);
                return;
            }
            switch(pres.Type)
            {
                case PresenceType.available:
                    if (pres.From.Server == "conference.server.octgn.info")
                    {
                        var rm = Chatting.GetRoom(new NewUser(pres.From), true);
                        rm.AddUser(new NewUser(pres.MucUser.Item.Jid),false);
                    }
                break;
                case PresenceType.unavailable:
                {
                    if (pres.From.Server == "conference.server.octgn.info")
                    {
                        if (pres.MucUser.Item.Jid == null) break;
                        if (pres.MucUser.Item.Jid.Bare == Me.User.Bare) break;
                        var rm = Chatting.GetRoom(new NewUser(pres.From),true);
                        rm.UserLeft(new NewUser(pres.MucUser.Item.Jid));
                    }
                    break;
                }
                case PresenceType.subscribe:
                    if (!Friends.Contains(new NewUser(pres.From.Bare)))
                    {
                        Notifications.Add(new FriendRequestNotification(pres.From.Bare , this , _noteId));
                        _noteId++;
                        if(OnFriendRequest != null) OnFriendRequest.Invoke(this , pres.From.Bare);
                    }
                    else
                        AcceptFriendship(pres.From.Bare);
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
            foreach(NewUser t in Friends) {
            	if(t.User.User != pres.From.User) continue;
            	t.CustomStatus = pres.Status ?? "";
            	t.SetStatus(pres);
            	break;
            }
        	XmppOnOnRosterEnd(this);
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
        	if(msg.Type != MessageType.normal) return;
        	switch(msg.Subject)
        	{
        		case "gameready":
        		{
        			int port;
        			if(Int32.TryParse(msg.Body , out port) && port != -1)
        			{
        				if(OnDataRecieved != null)
        					OnDataRecieved.Invoke(this , DataRecType.HostedGameReady , port);
        				CurrentHostedGamePort = port;
        			}
        		}
        			break;
        		case "gamelist":
        		{
        			var list = new List<HostedGameData>();
        			foreach( var a in msg.ChildNodes)
        			{
        				var gi = a as HostedGameData;
        				if(gi != null)
        					list.Add(gi);
        			}
        			_games = list;
        			if(OnDataRecieved != null)
        				OnDataRecieved.Invoke(this,DataRecType.GameList, list);
        		}
        			break;
        		case "refresh":
        			if(OnDataRecieved!=null)
        				OnDataRecieved.Invoke(this,DataRecType.GamesNeedRefresh,null);
        			break;
        		default:
        			if(msg.From.Bare.ToLower() == _xmpp.MyJID.Server.ToLower())
        			{
        				if (msg.Subject == null) msg.Subject = "";
        				if (msg.Body == null) msg.Body = "";
        				var d = new Dictionary<string , string>();
        				d["Message"] = msg.Body;
        				d["Subject"] = msg.Subject;
        				if(OnDataRecieved != null)
        					OnDataRecieved.Invoke(this,DataRecType.Announcement, d);
        			}
        			break;
        	}
        }

    	private void XmppOnOnRosterStart(object sender)
        {
            Friends.Clear();
        }

        private void XmppOnOnRosterEnd(object sender)
        {
            foreach(var n in Friends)
            {
                var viq = new VcardIq{Type = IqType.get , To = n.User.Bare};
                viq.GenerateId();
                _xmpp.Send(viq);
            }
            if(OnDataRecieved != null)
                OnDataRecieved.Invoke(this,DataRecType.FriendList,Friends);
            if (Chatting.Rooms.Count(x => x.IsGroupChat && x.GroupUser.User.Bare == "lobby@conference.server.octgn.info") == 0)
                _xmpp.RosterManager.AddRosterItem(new Jid("lobby@conference.server.octgn.info"));
        }

        private void XmppOnOnRosterItem(object sender, RosterItem item)
        {
            //Friends.Add(item.);
            switch(item.Subscription)
            {
                case SubscriptionType.none:
                    if (item.Jid.Server == "conference.server.octgn.info")
                    {
                        Chatting.GetRoom(new NewUser(item.Jid),true);
                    }
                    break;
                case SubscriptionType.to:
                    if(Friends.Count(x=>x.User.User == item.Jid.User) == 0)
                        Friends.Add(new NewUser(item.Jid));
                    break;
                case SubscriptionType.from:
                    if(Friends.Count(x=>x.User.User == item.Jid.User) == 0)
                    Friends.Add(new NewUser(item.Jid));
                    break;
                case SubscriptionType.both:
                    if(Friends.Count(x=>x.User.User == item.Jid.User) == 0)
                    Friends.Add(new NewUser(item.Jid));
                    break;
                case SubscriptionType.remove:
                    if (Friends.Contains(new NewUser(item.Jid)))
                        Friends.Remove(new NewUser(item.Jid));
                    break;
            }
        }

        private void XmppOnOnAuthError(object sender, Element element)
        {
            if(OnLoginComplete != null)
                OnLoginComplete.Invoke(this,LoginResults.Failure);
            Trace.WriteLine("[XMPP]AuthError: Closing...");
            _xmpp.Close();
        }

        private void XmppOnOnLogin(object sender)
        {
            _myPresence.Type = PresenceType.available;
            MucManager = new MucManager(_xmpp);
            Jid room = new Jid("lobby@conference.server.octgn.info");
            MucManager.AcceptDefaultConfiguration(room);
            MucManager.JoinRoom(room,Username,Password,false);
            Me = new NewUser(_xmpp.MyJID);
            if(OnLoginComplete != null)
                OnLoginComplete.Invoke(this,LoginResults.Success);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            Trace.WriteLine("[Xmpp]State: " + state.ToString());
            if (OnStateChanged != null)
                OnStateChanged.Invoke(this, state.ToString());
            if(state == XmppConnectionState.Disconnected)
                if(OnDisconnect != null)OnDisconnect.Invoke(this,null);
        }

        private void XmppOnOnRegisterError(object sender, Element element)
        {
            OnRegisterComplete.Invoke(this,RegisterResults.UsernameTaken);
            Trace.WriteLine("[Xmpp]Register Error...Closing...");
            _xmpp.Close();
        }

        private void XmppOnOnRegistered(object sender)
        {
            Vcard v = new Vcard();
            v.AddEmailAddress(new Email(EmailType.HOME, _email,true));
            v.JabberId = new Jid(Username + "@server.octgn.info");
            VcardIq vc = new VcardIq(IqType.set,v);
            _xmpp.IqGrabber.SendIq(vc);
            if(OnRegisterComplete != null)
                OnRegisterComplete.Invoke(this,RegisterResults.Success);
        }

        #endregion 

        public void Send(Element e)
        {
            _xmpp.Send(e);
        }
        
        public void Send(string s)
        {
            _xmpp.Send(s);
        }
        
        public void BeginLogin(string username, string password)
        {
            if (_xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                Username = username;
                Password = password;
                _xmpp.RegisterAccount = false;
                _xmpp.AutoAgents = true;
                _xmpp.AutoPresence = true;
                _xmpp.AutoRoster = true;
                _xmpp.Username = username;
                _xmpp.Password = password;
                _xmpp.Priority = 1;
                _xmpp.SocketConnectionType = SocketConnectionType.Direct;
                _xmpp.UseSSL = false;
                _xmpp.Open();
            }
        }

        public void BeginRegister(string username, string password, string email)
        {
            if (_xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                Username = username;
                Password = password;
                _xmpp.RegisterAccount = true;
                _xmpp.Username = username;
                _xmpp.Password = password;
                _email = email;
                _xmpp.Open();
            }
        }
		//TODO Uncomment
		/*
        public void BeginHostGame(Game game, string gamename)
        {
            var data = String.Format("{0},:,{1},:,{2}",game.Id.ToString(),game.Version.ToString(),gamename);
            var m = new Message(new Jid("gameserv@server.octgn.info"), Me.User, MessageType.normal, data, "hostgame");
            m.GenerateId();
            Xmpp.Send(m);
        }
		*/
        public void BeginGetGameList() 
        {
            var m = new Message(new Jid("gameserv@server.octgn.info"), MessageType.normal, "", "gamelist");
            m.GenerateId();
            _xmpp.Send(m);
        }

        public void BeginReconnect()
        {
            //Xmpp.Close();
            RebuildXmpp();
            BeginLogin(Username,Password);
            /*
            switch(Xmpp.XmppConnectionState)
            {
                case XmppConnectionState.Disconnected:
                    myPresence.Type = PresenceType.available;
                    Xmpp.Open();
                    Trace.WriteLine("[Xmpp]Reconnect: Opening");
                    break;
                default:
                    Trace.WriteLine("[Xmpp]Reconnect: Closing");
                    Xmpp.Close();
                    Xmpp.SocketDisconnect();
                    Xmpp.ClientSocket.Disconnect();
                    break;
            }*/
        }

        public void AcceptFriendship(Jid user)
        {
            _xmpp.PresenceManager.ApproveSubscriptionRequest(user);
            _xmpp.PresenceManager.Subscribe(user);
            _xmpp.SendMyPresence();
            if(OnDataRecieved != null)
                OnDataRecieved.Invoke(this,DataRecType.FriendList,Friends);
            //Xmpp.RequestRoster();
        }
        
        public void DeclineFriendship(Jid user)
        {
            _xmpp.PresenceManager.RefuseSubscriptionRequest(user);
        }
        
        public Notification[] GetNotificationList()
        {
            return Notifications.ToArray();
        }
        
        public void SetCustomStatus(string status)
        {
            _xmpp.Status = status;
            _xmpp.SendMyPresence();
        }
        
        public void SetStatus(UserStatus status)
        {
            Presence p;
            switch (status)
            {
                case UserStatus.Online:
                    p = new Presence(ShowType.NONE, _xmpp.Status)
                    {
                    	Type = PresenceType.available
                    };
            		_xmpp.Send(p);
                    break;
                case UserStatus.Away:
                    p = new Presence(ShowType.away, _xmpp.Status)
                    {
                    	Type = PresenceType.available
                    };
            		_xmpp.Send(p);
                    break;
                case UserStatus.DoNotDisturb:
                    p = new Presence(ShowType.dnd, _xmpp.Status)
                    {
                    	Type = PresenceType.available
                    };
            		_xmpp.Send(p);
                    break;
                case UserStatus.Invisible:                    
                    p = new Presence(ShowType.NONE, _xmpp.Status)
                    {
                    	Type = PresenceType.invisible
                    };
            		_xmpp.Send(p);
                    break;
            }
            Me.SetStatus(status);
        }
        
        public void SendFriendRequest(string username)
        {
            username = username.ToLower();
            if (username == Me.User.User.ToLower()) return;
            Jid j = new Jid(username,_xmpp.Server,"");

            _xmpp.RosterManager.AddRosterItem(j);
            
            _xmpp.PresenceManager.Subscribe(j);
        }
        
        public void RemoveFriend(NewUser user)
        {
            _xmpp.PresenceManager.Unsubscribe(user.User);
            RosterManager.RemoveRosterItem(user.User);
            Friends.Remove(user);
            OnDataRecieved.Invoke(this,DataRecType.FriendList, this);
        }
        
        public HostedGameData[] GetHostedGames() { return _games.ToArray(); }
        
        public void HostedGameStarted()
        {
            var m = new Message("gameserv@server.octgn.info", MessageType.normal, CurrentHostedGamePort.ToString(CultureInfo.InvariantCulture),
                                "gamestarted");
            _xmpp.Send(m);
        }
        
        public void Stop()
        {
            Trace.WriteLine("[Lobby]Stop Called.");
            RebuildXmpp();
        }
    }
}
