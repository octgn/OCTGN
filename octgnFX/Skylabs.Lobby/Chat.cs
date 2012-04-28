using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.chatstates;
using agsXMPP.protocol.x.muc;

namespace Skylabs.Lobby
{
    public class Chat
    {
        public delegate void dCreateChatRoom(object sender , NewChatRoom room);

        public event dCreateChatRoom OnCreateRoom;

        private long _lastRid = 0;
        private Client _client;
        public ThreadSafeList<NewChatRoom> Rooms; 
        private XmppClientConnection _xmpp;
        public long NextRid
        {
            get
            {
                _lastRid = _lastRid + 1 == long.MaxValue ? 0 : _lastRid+1;
                return _lastRid;
            }
        }

        public Chat(Client c, XmppClientConnection xmpp)
        {
            _client = c;
            Rooms = new ThreadSafeList<NewChatRoom>();
            _xmpp = xmpp;
            _xmpp.OnMessage += XmppOnOnMessage;
        }

        public void RemoveRoom(NewChatRoom room) { Rooms.Remove(room); }

        private void XmppOnOnMessage(object sender , Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.normal:
                    if(msg.From.Server == "conference.skylabsonline.com")
                    {
                        var rname = msg.From.User;
                        MucManager m = new MucManager(_xmpp);
                        m.JoinRoom(msg.From , msg.From.User);
                        var theRoom = GetRoom(new NewUser(msg.From) , true);
                        _xmpp.RosterManager.AddRosterItem(msg.From,msg.From.User);
                        if(OnCreateRoom != null)
                            OnCreateRoom.Invoke(this,theRoom);

                    }
                    break;
                case MessageType.error:
                {
                    var nc = GetRoom(new NewUser(msg.From.Bare) , true);
                    nc.OnMessage(this , msg);
                    break;
                }
                case MessageType.chat:
                {
                    switch(msg.Chatstate)
                    {
                        case Chatstate.None:
                            var nc = GetRoom(new NewUser(msg.From.Bare));
                            nc.OnMessage(sender , msg);
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
                }
                case MessageType.groupchat:
                {
                    var nc = GetRoom(new NewUser(msg.From.Bare) , true);
                    nc.OnMessage(this , msg);
                    break;
                }
                case MessageType.headline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public NewChatRoom GetRoom(NewUser otherUser, bool group=false)
        {
            if(group)
            {
                var ret = Rooms.SingleOrDefault(x => x.IsGroupChat && x.GroupUser.Equals(otherUser) );
                if(ret == null)
                {
                    ret = new NewChatRoom(NextRid , _client , otherUser);
                    Rooms.Add(ret);
                    if (OnCreateRoom != null) OnCreateRoom(this , ret);
                }
                return ret;
            }
            else
            {
                var ret = Rooms.SingleOrDefault(x => x.Users.Contains(otherUser) && !x.IsGroupChat);
                if(ret == null)
                {
                    ret = new NewChatRoom(NextRid,_client,otherUser);
                    Rooms.Add(ret);
                    if (OnCreateRoom != null) OnCreateRoom(this , ret);
                }
                
                return ret;
            }
        }
    }
}
