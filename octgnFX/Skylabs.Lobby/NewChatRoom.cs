using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.component;
using agsXMPP.protocol.extensions.chatstates;
using agsXMPP.protocol.x;
using Message = agsXMPP.protocol.client.Message;

namespace Skylabs.Lobby
{
    public class NewChatRoom: IDisposable
    {
        public delegate void dMessageReceived(object sender , NewUser from, string message);

        public event dMessageReceived OnMessageRecieved;

        public bool IsGroupChat { get; private set; }
        public long RID { get; private set; }
        public List<NewUser> Users { get; private set; }
        public NewUser GroupUser { get; private set; }
        private XmppClientConnection _xmpp;

        public NewChatRoom(long rid, XmppClientConnection xmpp,NewUser user)
        {
            RID = rid;
            Users = new List<NewUser>();
            _xmpp = xmpp;
            _xmpp.OnMessage += OnMessage;
            _xmpp.MessageGrabber.Add(_xmpp.MyJID,new BareJidComparer(), new MessageCB(MessageCallBack),null );
            if (user.User.Server == "conference.skylabsonline.com")
            {
                IsGroupChat = true;
                GroupUser = new NewUser(new Jid(user.User.Bare));
            }
            else Users.AddRange(new List<NewUser>(2){user,new NewUser(new Jid(_xmpp.MyJID.Bare))});
        }

        private void MessageCallBack(object sender , Message msg , object data)
        {
            //Debug.WriteLine("mcall:" + msg);
        }

        public void SendMessage(string message)
        {
            NewUser to;
            to = IsGroupChat ? GroupUser : Users.SingleOrDefault(x => x.User.Bare != _xmpp.MyJID.Bare);
            if(to == null) return;

            var j = new Jid(to.User );
            var m = new Message(j , (IsGroupChat)?MessageType.groupchat : MessageType.chat , message);
            m.GenerateId();
            m.XEvent = new Event();
            m.XEvent.Delivered = true;
            m.XEvent.Displayed = true;
            _xmpp.Send(m);
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

        public void Dispose() { _xmpp.OnMessage -= OnMessage; }
    }
}
