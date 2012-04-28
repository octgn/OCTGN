using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Octgn.Data;
using agsXMPP;
using agsXMPP.Factory;
using agsXMPP.Net;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.agent;
using agsXMPP.protocol.iq.register;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.vcard;
using agsXMPP.protocol.x.muc;

namespace Skylabs.Lobby
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
            public delegate void dRegisterComplete(object sender, RegisterResults results);
            public delegate void dStateChanged(object sender, string state);
            public delegate void dFriendRequest(object sender, Jid user);
            public delegate void dLoginComplete(object sender, LoginResults results);
            public delegate void dDataRecieved(object sender, DataRecType type, object data);
        #endregion
        #region Events
            public event dRegisterComplete OnRegisterComplete;
            public event dLoginComplete OnLoginComplete;
            public event dStateChanged OnStateChanged;
            public event dFriendRequest OnFriendRequest;
            public event dDataRecieved OnDataRecieved;
            public event EventHandler OnDisconnect;
        #endregion
        #region PrivateAccessors
            private XmppClientConnection Xmpp;
            private int _noteId = 0;
            private Presence myPresence;
            private List<HostedGameData> _games;
            private string _email;
        #endregion

        public List<Notification> Notifications { get; set; }
        public List<NewUser> Friends { get; set; }
        //public List<NewUser> GroupChats { get; set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string CustomStatus { get { return Xmpp.Status; }set{SetCustomStatus(value);} }
        public MucManager MucManager { get; set; }
        public RosterManager RosterManager { get { return Xmpp.RosterManager; } }
        public NewUser Me { get; private set; }
        public Chat Chatting { get; set; }
        public int CurrentHostedGamePort { get; set; }
        public bool DisconnectedBecauseConnectionReplaced { get; set; }

        public UserStatus Status
        {
            get { return NewUser.PresenceToStatus(myPresence); }
            set { SetStatus(value); }
        }

        
        public Client()
        {
            RebuildXmpp();
        }

        private void RebuildXmpp()
        {
            if(Xmpp != null)
            {
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                Xmpp.Close();
                Xmpp = null;
            }
            DisconnectedBecauseConnectionReplaced = false;
            Xmpp = new XmppClientConnection("skylabsonline.com");
            Xmpp.OnRegistered += XmppOnOnRegistered;
            Xmpp.OnRegisterError += XmppOnOnRegisterError;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.OnLogin += XmppOnOnLogin;
            Xmpp.OnAuthError += XmppOnOnAuthError;
            Xmpp.OnRosterItem += XmppOnOnRosterItem;
            Xmpp.OnRosterEnd += XmppOnOnRosterEnd;
            Xmpp.OnRosterStart += XmppOnOnRosterStart;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnPresence += XmppOnOnPresence;
            Xmpp.OnAgentItem += XmppOnOnAgentItem;
            Xmpp.OnIq += XmppOnOnIq;
            Xmpp.OnReadXml += XmppOnOnReadXml;
            Xmpp.OnClose += XmppOnOnClose;
            Xmpp.OnWriteXml += XmppOnOnWriteXml;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnSocketError += XmppOnOnSocketError;
            Xmpp.OnStreamError += XmppOnOnStreamError;
            Notifications = new List<Notification>();
            Friends = new List<NewUser>();
            //GroupChats = new List<NewUser>();
            myPresence = new Presence();
            Chatting = new Chat(this, Xmpp);
            CurrentHostedGamePort = -1;
            _games = new List<HostedGameData>();
            agsXMPP.Factory.ElementFactory.AddElementType("gameitem", "octgn:gameitem", typeof(HostedGameData));
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
            if (pres.From.User == Xmpp.MyJID.User)
            {
                myPresence = pres;
                myPresence.Type = PresenceType.available;
                Xmpp.Status = myPresence.Status ?? Xmpp.Status;
                if(OnDataRecieved != null)
                    OnDataRecieved.Invoke(this,DataRecType.MyInfo, pres);
                return;
            }
            switch(pres.Type)
            {
                case PresenceType.available:                    
                    if(pres.From.Server == "conference.skylabsonline.com")
                    {
                        var rm = Chatting.GetRoom(new NewUser(pres.From), true);
                        rm.AddUser(new NewUser(pres.MucUser.Item.Jid),false);
                    }
                break;
                case PresenceType.unavailable:
                {
                    if(pres.From.Server == "conference.skylabsonline.com")
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
            for(int i=0;i<Friends.Count;i++)
            {
                if(Friends[i].User.User == pres.From.User)
                {
                    Friends[i].CustomStatus = pres.Status ?? "";
                    Friends[i].SetStatus(pres);
                    break;
                }
            }
            XmppOnOnRosterEnd(this);
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            if(msg.Type == MessageType.normal)
            {
                if (msg.Subject == "gameready")
                {
                    var port = -1;
                    if(Int32.TryParse(msg.Body , out port) && port != -1)
                    {
                        if(OnDataRecieved != null)
                            OnDataRecieved.Invoke(this , DataRecType.HostedGameReady , port);
                        CurrentHostedGamePort = port;
                    }
                }
                else if(msg.Subject == "gamelist")
                {
                    var list = new List<HostedGameData>();
                    foreach( var a in msg.ChildNodes)
                    {
                        var gi = a as HostedGameData;
                        if(gi != null)
                            list.Add(gi);
                        var el = a as Element;
                        gi = el as HostedGameData;
                        if (el == null) continue;
                    }
                    _games = list;
                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(this,DataRecType.GameList, list);
                }
                else if(msg.Subject == "refresh")
                {
                    if(OnDataRecieved!=null)
                        OnDataRecieved.Invoke(this,DataRecType.GamesNeedRefresh,null);
                }
                else if(msg.From.Bare.ToLower() == Xmpp.MyJID.Server.ToLower())
                {
                    if (msg.Subject == null) msg.Subject = "";
                    if (msg.Body == null) msg.Body = "";
                    var d = new Dictionary<string , string>();
                    d["Message"] = msg.Body;
                    d["Subject"] = msg.Subject;
                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(this,DataRecType.Announcement, d);
                }
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
                Xmpp.Send(viq);
            }
            if(OnDataRecieved != null)
                OnDataRecieved.Invoke(this,DataRecType.FriendList,Friends);
            if(Chatting.Rooms.Count(x=>x.IsGroupChat && x.GroupUser.User.Bare == "lobby@conference.skylabsonline.com") == 0)
                Xmpp.RosterManager.AddRosterItem(new Jid("lobby@conference.skylabsonline.com"));
        }

        private void XmppOnOnRosterItem(object sender, RosterItem item)
        {
            //Friends.Add(item.);
            switch(item.Subscription)
            {
                case SubscriptionType.none:
                    if (item.Jid.Server == "conference.skylabsonline.com")
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
            Xmpp.Close();
        }

        private void XmppOnOnLogin(object sender)
        {
            myPresence.Type = PresenceType.available;
            MucManager = new MucManager(Xmpp);
            Jid room = new Jid("lobby@conference.skylabsonline.com");
            MucManager.AcceptDefaultConfiguration(room);
            MucManager.JoinRoom(room,Username,Password,false);
            Me = new NewUser(Xmpp.MyJID);
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
            Xmpp.Close();
        }

        private void XmppOnOnRegistered(object sender)
        {
            Vcard v = new Vcard();
            v.AddEmailAddress(new Email(EmailType.HOME, _email,true));
            v.JabberId = new Jid(this.Username + "@skylabsonline.com");
            VcardIq vc = new VcardIq(IqType.set,v);
            Xmpp.IqGrabber.SendIq(vc);
            if(OnRegisterComplete != null)
                OnRegisterComplete.Invoke(this,RegisterResults.Success);
        }

        #endregion 

        public void Send(Element e)
        {
            Xmpp.Send(e);
        }
        
        public void Send(string s)
        {
            Xmpp.Send(s);
        }
        
        public void BeginLogin(string username, string password)
        {
            if (Xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                Username = username;
                Password = password;
                Xmpp.RegisterAccount = false;
                Xmpp.AutoAgents = true;
                Xmpp.AutoPresence = true;
                Xmpp.AutoRoster = true;
                Xmpp.Username = username;
                Xmpp.Password = password;
                Xmpp.Priority = 1;
                Xmpp.SocketConnectionType = SocketConnectionType.Direct;
                Xmpp.UseSSL = false;
                Xmpp.Open();
            }
        }

        public void BeginRegister(string username, string password, string email)
        {
            if (Xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                Username = username;
                Password = password;
                Xmpp.RegisterAccount = true;
                Xmpp.Username = username;
                Xmpp.Password = password;
                _email = email;
                Xmpp.Open();
            }
        }

        public void BeginHostGame(Game game, string gamename)
        {
            var data = String.Format("{0},:,{1},:,{2}",game.Id.ToString(),game.Version.ToString(),gamename);
            var m = new Message(new Jid("gameserv@skylabsonline.com"),Me.User,MessageType.normal,data,"hostgame");
            m.GenerateId();
            Xmpp.Send(m);
        }

        public void BeginGetGameList() 
        { 
            var m = new Message(new Jid("gameserv@skylabsonline.com") , MessageType.normal , "" , "gamelist");
            m.GenerateId();
            Xmpp.Send(m);
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
            Xmpp.PresenceManager.ApproveSubscriptionRequest(user);
            Xmpp.PresenceManager.Subscribe(user);
            Xmpp.SendMyPresence();
            if(OnDataRecieved != null)
                OnDataRecieved.Invoke(this,DataRecType.FriendList,Friends);
            //Xmpp.RequestRoster();
        }
        
        public void DeclineFriendship(Jid user)
        {
            Xmpp.PresenceManager.RefuseSubscriptionRequest(user);
        }
        
        public Notification[] GetNotificationList()
        {
            return Notifications.ToArray();
        }
        
        public void SetCustomStatus(string status)
        {
            Xmpp.Status = status;
            Xmpp.SendMyPresence();
        }
        
        public void SetStatus(UserStatus status)
        {
            Presence p;
            switch (status)
            {
                case UserStatus.Online:
                    p = new Presence(ShowType.NONE, Xmpp.Status);
                    p.Type = PresenceType.available;
                    Xmpp.Send(p);
                    break;
                case UserStatus.Away:
                    p = new Presence(ShowType.away, Xmpp.Status);
                    p.Type = PresenceType.available;
                    Xmpp.Send(p);
                    break;
                case UserStatus.DoNotDisturb:
                    p = new Presence(ShowType.dnd, Xmpp.Status);
                    p.Type = PresenceType.available;
                    Xmpp.Send(p);
                    break;
                case UserStatus.Invisible:                    
                    p = new Presence(ShowType.NONE, Xmpp.Status);
                    p.Type = PresenceType.invisible;
                    Xmpp.Send(p);
                    break;
            }
            Me.SetStatus(status);
        }
        
        public void SendFriendRequest(string username)
        {
            username = username.ToLower();
            if (username == Me.User.User.ToLower()) return;
            Jid j = new Jid(username,Xmpp.Server,"");

            Xmpp.RosterManager.AddRosterItem(j);
            
            Xmpp.PresenceManager.Subscribe(j);
        }
        
        public void RemoveFriend(NewUser user)
        {
            Xmpp.PresenceManager.Unsubscribe(user.User);
            RosterManager.RemoveRosterItem(user.User);
            Friends.Remove(user);
            OnDataRecieved.Invoke(this,DataRecType.FriendList, this);
        }
        
        public HostedGameData[] GetHostedGames() { return _games.ToArray(); }
        
        public void HostedGameStarted()
        {
            var m = new Message("gameserv@skylabsonline.com" , MessageType.normal , CurrentHostedGamePort.ToString() ,
                                "gamestarted");
            Xmpp.Send(m);
        }
        
        public void Stop()
        {
            Trace.WriteLine("[Lobby]Stop Called.");
            RebuildXmpp();
        }
    }
}
