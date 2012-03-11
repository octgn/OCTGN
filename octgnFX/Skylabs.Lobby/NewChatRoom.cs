using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using agsXMPP.protocol.extensions.chatstates;

namespace Skylabs.Lobby
{
    public class NewChatRoom: IDisposable
    {
        public delegate void dMessageReceived(object sender , NewUser from, string message);

        public event dMessageReceived OnMessageRecieved;

        public bool IsGroupChat { get; private set; }
        public long RID { get; private set; }
        public List<NewUser> Users { get; private set; }
        private XmppClientConnection _xmpp;

        public NewChatRoom(long rid, XmppClientConnection xmpp,params NewUser[] users)
        {
            RID = rid;
            Users = new List<NewUser>();
            _xmpp = xmpp;
            _xmpp.OnMessage += OnMessage;
            _xmpp.MessageGrabber.Add(_xmpp.MyJID,new BareJidComparer(), new MessageCB(MessageCallBack),null );
            if (users.Length > 2) IsGroupChat = true;
            //TODO Need more group chat logic.
            Users.AddRange(users);
        }

        private void MessageCallBack(object sender , Message msg , object data)
        {
            //Debug.WriteLine("mcall:" + msg);
        }

        public void SendMessage(string message)
        {
            if(IsGroupChat)
            {
                //TODO Add group chat sending shit.
            }
            else
            {
                var to = Users.SingleOrDefault(x => x.User.User != _xmpp.MyJID.User);
                if (to != null)
                {
                    var j = new Jid(to.User );
                    _xmpp.Send(new Message(j , MessageType.chat , message));
                    //_xmpp.Send(new Message(_xmpp.MyJID,MessageType.chat,message));
                }
            }
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
                            if(!String.IsNullOrWhiteSpace(msg.Body) && OnMessageRecieved != null)
                                OnMessageRecieved.Invoke(this,new NewUser(msg.From),msg.Body );
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
