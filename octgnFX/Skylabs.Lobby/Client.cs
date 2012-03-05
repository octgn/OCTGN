using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.agent;
using agsXMPP.protocol.iq.register;
using agsXMPP.protocol.iq.roster;

namespace Skylabs.Lobby
{
    public class Client
    {
        #region Enums
            public enum RegisterResults{ConnectionError,Success,UsernameTaken,UsernameInvalid,PasswordFailure}
            public enum LoginResults{ConnectionError,Success,Failure}
            public enum DataRecType{FriendList,MyInfo}
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
        #endregion
        #region PrivateAccessors
            private XmppClientConnection Xmpp;
            private int _noteId = 0;
            private Presence myPresence;
        #endregion

        public List<Notification> Notifications { get; set; }
        public List<NewUser> Friends { get; set; } 
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string CustomStatus { get { return Xmpp.Status; }set{SetCustomStatus(value);} }
        public UserStatus Status
        {
            get { return NewUser.PresenceToStatus(myPresence); }
            set { SetStatus(value); }
        }

        
        public Client()
        {
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
            Notifications = new List<Notification>();
            Friends = new List<NewUser>();
            myPresence = new Presence();
        }

        private void XmppOnOnIq(object sender, IQ iq)
        {
            Debug.WriteLine(iq);
        }

        private void XmppOnOnAgentItem(object sender, Agent agent)
        {
            Debug.WriteLine(agent);
        }

        private void XmppOnOnPresence(object sender, Presence pres)
        {
            Debug.WriteLine(pres);
            if (pres.From.User == Xmpp.MyJID.User)
            {
                myPresence = pres;
                Xmpp.Status = myPresence.Status ?? Xmpp.Status;
                if(OnDataRecieved != null)
                    OnDataRecieved.Invoke(this,DataRecType.MyInfo, pres);
                return;
            }
            switch(pres.Type)
            {
                case PresenceType.subscribe:
                    Notifications.Add(new FriendRequestNotification(pres.From,this,_noteId));
                    _noteId++;
                    if(OnFriendRequest != null)
                        OnFriendRequest.Invoke(this,pres.From);
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
            var f = Friends.SingleOrDefault(x => x.User.User == pres.From.User);
            if (f == null) return;
            f.CustomStatus = pres.Status ?? "";
            f.SetStatus(pres);
            XmppOnOnRosterEnd(this);
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        private void XmppOnOnRosterStart(object sender)
        {
            Friends.Clear();
        }

        private void XmppOnOnRosterEnd(object sender)
        {
            if(OnDataRecieved != null)
                OnDataRecieved.Invoke(this,DataRecType.FriendList,Friends);
        }

        private void XmppOnOnRosterItem(object sender, RosterItem item)
        {
            //Friends.Add(item.);
            switch(item.Subscription)
            {
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
            Xmpp.Close();
        }

        private void XmppOnOnLogin(object sender)
        {
            if(OnLoginComplete != null)
                OnLoginComplete.Invoke(this,LoginResults.Success);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            if (OnStateChanged != null)
                OnStateChanged.Invoke(this, state.ToString());
        }

        private void XmppOnOnRegisterError(object sender, Element element)
        {
            OnRegisterComplete.Invoke(this,RegisterResults.UsernameTaken);
            Xmpp.Close();
        }

        private void XmppOnOnRegistered(object sender)
        {
            if(OnRegisterComplete != null)
                OnRegisterComplete.Invoke(this,RegisterResults.Success);
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
                Xmpp.Open();
            }
        }

        public void BeginRegister(string username, string password)
        {
            if (Xmpp.XmppConnectionState == XmppConnectionState.Disconnected)
            {
                Username = username;
                Password = password;
                Xmpp.RegisterAccount = true;
                Xmpp.Username = username;
                Xmpp.Password = password;
                Xmpp.Open();
            }
        }

        public void AcceptFriendship(Jid user)
        {
            Xmpp.PresenceManager.ApproveSubscriptionRequest(user);
            Xmpp.PresenceManager.Subscribe(user);
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
        }
        public void SendFriendRequest(string username)
        {
            Jid j = new Jid(username,Xmpp.Server,"");

            Xmpp.RosterManager.AddRosterItem(j);
            
            Xmpp.PresenceManager.Subscribe(j);
        }
        public void RemoveFriend(NewUser user)
        {
            Xmpp.PresenceManager.Unsubscribe(user.User);
        }
    }
}
