using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;

namespace Skylabs.LobbyServer
{
    public static class GameBot
    {
        private static XmppClientConnection Xmpp { get; set; }
        private static ThreadSafeList<Jid> _userList { get; set; }
        public static ObjectHandler OnCheckRecieved { get; set; }
        public static void Init() 
        {  }
        static GameBot() 
        {
            RemakeXmpp();
        }
        public static void RemakeXmpp()
        {
            if (Xmpp != null)
            {
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                Xmpp.Close();
                Xmpp = null;
            }
            Xmpp = new XmppClientConnection("skylabsonline.com");
            //#else
            //            Xmpp = new XmppClientConnection();
            //            Xmpp.ConnectServer = "127.0.0.1";
            //            Xmpp.AutoResolveConnectServer = false;
            //#endif            

            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.Username = "gameserv";
            Xmpp.Password = "12345";//Don't commit real password
            Xmpp.Priority = 1;
            Xmpp.OnLogin += XmppOnOnLogin;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnIq += XmppOnOnIq;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnAuthError += new XmppElementHandler(Xmpp_OnAuthError);
            Xmpp.OnReadXml += XmppOnOnReadXml;
            Xmpp.OnPresence += XmppOnOnPresence;
            Xmpp.OnWriteXml += XmppOnOnWriteXml;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            _userList = new ThreadSafeList<Jid>();
            Xmpp.Open();
        }

        private static void XmppOnOnAgentStart(object sender) 
        {
            if(OnCheckRecieved != null)
                OnCheckRecieved.Invoke(sender);
        }

        public static void CheckBotStatus()
        {
            Xmpp.RequestAgents();
        }
        private static void XmppOnOnWriteXml(object sender , string xml)
        {
            
        }

        private static void XmppOnOnPresence(object sender , Presence pres)
        {
            Trace.WriteLine("[Bot]OnPresence");
            switch (pres.Type)
            {
                case PresenceType.available:
                    if(pres.From.Server == "conference.skylabsonline.com")
                    {
                        if (!_userList.Contains(pres.MucUser.Item.Jid) && pres.MucUser.Item.Jid != Xmpp.MyJID)
                            _userList.Add(pres.MucUser.Item.Jid);
                    }
                    break;
                case PresenceType.unavailable:
                {
                    if (pres.From.Server == "conference.skylabsonline.com")
                    {
                        _userList.Remove(pres.MucUser.Item.Jid);
                    }
                    break;
                }
            }
            Trace.WriteLine("[Bot]OnPresenceEnd");
        }

        private static void XmppOnOnReadXml(object sender , string xml)
        {
            Trace.WriteLine("[Bot]RawData:\n" + xml + "\n");
        }

        static void Xmpp_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            Trace.WriteLine("[Bot]AuthError:" + e);
        }

        private static void XmppOnOnError(object sender , Exception exception)
        {
            Trace.WriteLine("[Bot]Error:" + exception.Message);
        }

        private static void XmppOnOnIq(object sender , IQ iq)
        {

        }

        private static void XmppOnOnXmppConnectionStateChanged(object sender , XmppConnectionState state)
        {
            Trace.WriteLine("[Bot]ConState:" + state.ToString());
            if(state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private static void XmppOnOnMessage(object sender , Message msg)
        {
            switch(msg.Type)
            {
                case MessageType.normal:
                    if(msg.Subject == "hostgame")
                    {
                        var data = msg.Body.Split(new string[1]{",:,"},StringSplitOptions.RemoveEmptyEntries);
                        if (data.Length != 3) return;
                        var guid = Guid.Empty;
                        Version ver = null;
                        if (String.IsNullOrWhiteSpace(data[2])) return;
                        var gameName = data[2];
                        if(Guid.TryParse(data[0] , out guid) && Version.TryParse(data[1] , out ver))
                        {
                            var port = Gaming.HostGame(guid , ver , gameName , "" , new NewUser(msg.From));
                            if (port == -1) return;
                            var m = new Message(msg.From , msg.To , MessageType.normal , port.ToString() , "gameready");
                            m.GenerateId();
                            Xmpp.Send(m);
                            var gameMessage = String.Format(" {0} is hosting a game called '{1}'" ,msg.From.User,gameName);
                            m = new Message(new Jid("lobby@conference.skylabsonline.com"),msg.To,MessageType.groupchat,gameMessage);
                            Xmpp.Send(m);
                            RefreshLists();
                        }
                    }
                    else if(msg.Subject == "gamelist")
                    {
                        //Trace.WriteLine("[Bot]Request GameList: " + msg.From.User);
                        var list = Gaming.GetLobbyList().Where(x=>x.GameStatus == EHostedGame.StartedHosting);
                        var m = new Message(msg.From , MessageType.normal , "" , "gamelist");
                        m.GenerateId();
                        foreach (var a in list)
                        {
                            m.AddChild(a);
                        }
                        Xmpp.Send(m);
                    }
                    else if(msg.Subject == "gamestarted")
                    {
                        int port = -1;
                        if(Int32.TryParse(msg.Body,out port))
                            Gaming.StartGame(port);
                        RefreshLists();
                    }
                    break;
                case MessageType.error:
                    break;
                case MessageType.chat:
                    break;
                case MessageType.groupchat:
                    break;
                case MessageType.headline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void RefreshLists()
        {
            Trace.WriteLine("[Bot]RefreshList");
            var arr = _userList.ToArray();
            if(arr == null) Trace.WriteLine("[Bot]arr=null");
            foreach(var u in arr)
            {
                if (u.Bare == null) continue;
                if (Xmpp.MyJID == null) { Trace.WriteLine("[Bot]Xmpp.MyJid == null");
                    continue;
                }
                if (Xmpp.MyJID.Bare == null) { Trace.WriteLine("[Bot]Xmpp.MyJid.Bare == null");
                    continue;
                }
                if (u.Bare == Xmpp.MyJID.Bare) continue;
                if (u.Resource.ToLower() != "agsxmpp") continue;
                var m = new Message(u.Bare , MessageType.normal , "" , "refresh");
                Xmpp.Send(m);
            }
            Trace.WriteLine("[Bot]RefreshListEnd");
        }

        private static void XmppOnOnLogin(object sender) 
        {
            Trace.WriteLine("[Bot]Login:" );
            var muc = new MucManager(Xmpp);
            Jid room = new Jid("lobby@conference.skylabsonline.com");
            muc.AcceptDefaultConfiguration(room);
            muc.JoinRoom(room, Xmpp.Username, Xmpp.Password, false);
        }
    }
}
