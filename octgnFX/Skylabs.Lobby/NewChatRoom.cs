using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.component;
using agsXMPP.protocol.extensions.chatstates;
using agsXMPP.protocol.x;
using agsXMPP.protocol.x.muc;
using Message = agsXMPP.protocol.client.Message;
using Presence = agsXMPP.protocol.client.Presence;

namespace Skylabs.Lobby
{
    public class NewChatRoom: IDisposable,IEquatable<NewChatRoom> 
    {
        public delegate void dMessageReceived(object sender , NewUser from, string message);

        public delegate void dUserListChange(object sender,List<NewUser> users );

        public event dMessageReceived OnMessageRecieved;
        public event dUserListChange OnUserListChange;

        public bool IsGroupChat { get; private set; }
        public long RID { get; private set; }
        public List<NewUser> Users { get; set; }
        public NewUser GroupUser { get; private set; }
        private Client _client;
        public NewChatRoom(long rid, Client c,NewUser user)
        {
            RID = rid;
            Users = new List<NewUser>();
            _client = c;
            if (user.User.Server == "conference.skylabsonline.com")
            {
                IsGroupChat = true;
                GroupUser = new NewUser(new Jid(user.User.Bare));
                _client.MucManager.JoinRoom(GroupUser.User,_client.Me.User.User);
            }
            else
                AddUser(user);
            AddUser(_client.Me);
        }
        public void UserLeft(NewUser user)
        {
            Users.Remove(user);
            if(OnUserListChange != null) OnUserListChange.Invoke(this , Users);
        }

        private void MessageCallBack(object sender , Message msg , object data)
        {
            //Debug.WriteLine("mcall:" + msg);
        }

        public void SendMessage(string message)
        {
            NewUser to;
            to = IsGroupChat ? GroupUser : Users.SingleOrDefault(x => x.User.Bare != _client.Me.User.Bare);
            if(to == null) return;

            var j = new Jid(to.User );
            var m = new Message(j , (IsGroupChat)?MessageType.groupchat : MessageType.chat , message);
            m.GenerateId();
            m.XEvent = new Event();
            m.XEvent.Delivered = true;
            m.XEvent.Displayed = true;
            _client.Send(m);
        }
        public  void OnMessage(object sender , Message msg) 
        {
            switch(msg.Type)
            {
                case MessageType.normal:
                    break;
                case MessageType.error:
                    break;
                case MessageType.chat:
                    switch(msg.Chatstate)
                    {
                        case Chatstate.None:
                            if(!IsGroupChat && !String.IsNullOrWhiteSpace(msg.Body) && OnMessageRecieved != null && Users.Contains(new NewUser(msg.From.Bare)))
                                OnMessageRecieved.Invoke(this,new NewUser(msg.From.Bare),msg.Body );
                            break;
                        case Chatstate.active:
                            
                            break;
                        case Chatstate.inactive:
                            break;
                        case Chatstate.composing:
                            break;
                        case Chatstate.gone:
                            break;
                        case Chatstate.paused:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case MessageType.groupchat:
                    if(IsGroupChat && msg.Chatstate == Chatstate.None)
                    {
                        if(msg.From.Bare == GroupUser.User.Bare)
                        {
                            if(!String.IsNullOrWhiteSpace(msg.Body))
                                if(OnMessageRecieved != null)
                                    OnMessageRecieved.Invoke(this,new NewUser(new Jid(msg.From.Resource + "@skylabsonline.com")),msg.Body );
                        }
                    }
                    break;
                case MessageType.headline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void MakeGroupChat(NewUser gcu) 
        { 
            IsGroupChat = true;
            GroupUser = gcu;
        }
        public void AddUser(NewUser user, bool InviteUser = true)
        {
            if(!Users.Contains(user)) Users.Add(user);
            if (Users.Count > 2 || IsGroupChat)
            {
                if (!IsGroupChat)
                {
                    IsGroupChat = true;
                    var rname = Randomness.RandomRoomName();
                    GroupUser = new NewUser(rname + "@conference.skylabsonline.com");

                    _client.MucManager.JoinRoom(GroupUser.User , _client.Me.User.User);
                    _client.RosterManager.AddRosterItem(GroupUser.User,GroupUser.User.User);
                    
                }
                if(InviteUser)
                    foreach(var u in Users) if(u != _client.Me) _client.MucManager.Invite(u.User , GroupUser.User);
                
            }

            if(OnUserListChange != null) OnUserListChange.Invoke(this,Users);
            

        }
        public void LeaveRoom()
        {
            if(IsGroupChat && GroupUser.User != "lobby")
            {
                _client.MucManager.LeaveRoom(GroupUser.User,GroupUser.User.User);
                _client.RosterManager.RemoveRosterItem(GroupUser.User);
            }
        }
        public void Dispose() { 

        }
        public bool Equals(NewChatRoom other) { return other.RID == RID; }
        public static bool operator==(NewChatRoom a, NewChatRoom b)
        {
            long rid1 = -1;
            long rid2 = -1;
            try
            {
                rid1 = a.RID;
            }
            catch{}
            try
            {
                rid2 = b.RID;
            }
            catch {}

            if (rid1 == -1 && rid2 == -1) return true;
            return rid1 == rid2;
        }
        public static bool operator !=(NewChatRoom a , NewChatRoom b) { return !(a == b); }
    }
}
