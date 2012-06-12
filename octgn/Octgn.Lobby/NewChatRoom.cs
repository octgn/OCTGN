using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.chatstates;
using agsXMPP.protocol.x;
using Message = agsXMPP.protocol.client.Message;

namespace Octgn.Lobby
{
    public class NewChatRoom: IDisposable,IEquatable<NewChatRoom>,IEqualityComparer
    {
        public delegate void DMessageReceived(object sender , NewUser from, string message, DateTime rTime,Client.LobbyMessageType mType = Client.LobbyMessageType.Standard);

        public delegate void DUserListChange(object sender,List<NewUser> users );

        public event DMessageReceived OnMessageRecieved;
        public event DUserListChange OnUserListChange;

        public bool IsGroupChat { get; private set; }
        public long Rid { get; private set; }
        public List<NewUser> Users { get; set; }
        public NewUser GroupUser { get; private set; }
        private readonly Client _client;
        public NewChatRoom(long rid, Client c,NewUser user)
        {
            Rid = rid;
            Users = new List<NewUser>();
            _client = c;
            if (user.User.Server == "conference.server.octgn.info")
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

    	public void SetTopic(string topic) 
        {
            if (!IsGroupChat || GroupUser == null)return;
            var m = new Message(GroupUser.User.Bare , MessageType.groupchat , "" , topic);
            m.GenerateId();
            m.XEvent = new Event
            {
            	Delivered = true ,
            	Displayed = true
            };
        	_client.Send(m);
        }

        public void SendMessage(string message)
        {
        	NewUser to = IsGroupChat ? GroupUser : Users.SingleOrDefault(x => x.User.Bare != _client.Me.User.Bare);
            if(to == null || String.IsNullOrWhiteSpace(message)) return;

            if(message[0] == '/')
            {
                message = message.Substring(1);
                var mend = message.IndexOf(' ');
                var command = message.Substring(0 , mend).ToLower();
                var mess = "";
                if(message.Length > command.Length+1)
                    mess = message.Substring(mend + 1);
                switch(command)
                {
                    case "topic":
                    {
                        SetTopic(mess);
                        break;
                    }
                }
            }
            else
            {
                var j = new Jid(to.User);
                var m = new Message(j, (IsGroupChat) ? MessageType.groupchat : MessageType.chat, message);
                m.GenerateId();
                m.XEvent = new Event
                {
                	Delivered = true ,
                	Displayed = true
                };
            	_client.Send(m);
            }

        }
        public  void OnMessage(object sender , Message msg) 
        {
            var rTime = DateTime.Now;
            if (msg.XDelay != null && msg.XDelay.Stamp != null) rTime = msg.XDelay.Stamp.ToLocalTime();
            switch(msg.Type)
            {
                case MessageType.normal:
                    break;
                case MessageType.error:
                    if(msg.Error != null && !String.IsNullOrWhiteSpace(msg.Error.ErrorText))
                        OnMessageRecieved.Invoke(this,new NewUser(msg.From),msg.Error.ErrorText,DateTime.Now,Client.LobbyMessageType.Error );
                    break;
                case MessageType.chat:
                    switch(msg.Chatstate)
                    {
                        case Chatstate.None:
                            if(!IsGroupChat && !String.IsNullOrWhiteSpace(msg.Body) && OnMessageRecieved != null && Users.Contains(new NewUser(msg.From.Bare)))
                                OnMessageRecieved.Invoke(this,new NewUser(msg.From.Bare),msg.Body,rTime );
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
                            if(!String.IsNullOrWhiteSpace(msg.Subject))
                            {
                                if (OnMessageRecieved != null)
                                    OnMessageRecieved.Invoke(this,
                                                             new NewUser(
                                                                 new Jid(msg.From.Resource + "@server.octgn.info")),
                                                             msg.Subject, rTime,Client.LobbyMessageType.Topic);                                
                            }
                            else if (!String.IsNullOrWhiteSpace(msg.Body))
                            {
                                if(OnMessageRecieved != null)
                                    OnMessageRecieved.Invoke(this ,
                                                             new NewUser(
                                                                 new Jid(msg.From.Resource + "@server.octgn.info")),
                                                             msg.Body , rTime);
                            }
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
        public void AddUser(NewUser user, bool inviteUser = true)
        {
            if(!Users.Contains(user)) Users.Add(user);
            if (Users.Count > 2 || IsGroupChat)
            {
                if (!IsGroupChat)
                {
                    IsGroupChat = true;
                    var rname = Randomness.RandomRoomName();
                    GroupUser = new NewUser(rname + "@conference.server.octgn.info");

                    _client.MucManager.JoinRoom(GroupUser.User , _client.Me.User.User);
                    _client.RosterManager.AddRosterItem(GroupUser.User,GroupUser.User.User);
                    
                }
                if(inviteUser)
                    foreach(var u in Users) if(u != _client.Me) _client.MucManager.Invite(u.User , GroupUser.User);
                
            }

            if(OnUserListChange != null) OnUserListChange.Invoke(this,Users);
            

        }
        public void LeaveRoom()
        {
            if(IsGroupChat && GroupUser.User != "lobby")
            {
                _client.MucManager.LeaveRoom(GroupUser.User.Bare,_client.Me.User.User);
                _client.RosterManager.RemoveRosterItem(GroupUser.User.Bare);
                _client.Chatting.RemoveRoom(this);
            }
        }
        public void Dispose() { 

        }
        public bool Equals(NewChatRoom other) { return other.Rid == Rid; }
        public override bool Equals(Object o) { return GetHashCode() == o.GetHashCode(); }
        public static bool operator==(NewChatRoom a, NewChatRoom b)
        {
            long rid1 = -1;
            long rid2 = -1;
            if ((object.Equals(a, null) && !object.Equals(b, null)) || (object.Equals(b, null) && !object.Equals(a, null))) return false;

            if (!object.Equals(a, null)) rid1 = a.Rid;
            if (!object.Equals(b, null)) rid2 = b.Rid;
            /*
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
            */
            if (rid1 == -1 && rid2 == -1) return true;
            return rid1 == rid2;
        }
        public static bool operator !=(NewChatRoom a , NewChatRoom b) { return !(a == b); }
        public new bool Equals(object x , object y) { return x.GetHashCode() == y.GetHashCode(); }
        public int GetHashCode(object obj) { return obj.GetHashCode(); }
        public override int GetHashCode() { return (int)Rid; }
    }
}
