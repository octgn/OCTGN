using System;
using System.Diagnostics;
using System.Linq;

using Skylabs.Lobby;
using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.client;

namespace Skylabs.LobbyServer
{
    using System.Collections.Generic;
    using System.Configuration;

    public static class GameBot
    {
        private static XmppClientConnection Xmpp { get; set; }

        private static readonly string ServerPath = "";
        private static readonly string XmppUsername = "";
        private static readonly string XmppPassword = "";

        public static ObjectHandler OnCheckRecieved { get; set; }

        public static void Init() 
        {  }
        static GameBot()
        {
            ServerPath = ConfigurationManager.AppSettings["ServerPath"];
            XmppUsername = ConfigurationManager.AppSettings["XmppUsername"];
            XmppPassword = ConfigurationManager.AppSettings["XmppPassword"];
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
            Xmpp = new XmppClientConnection(ServerPath);

            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.Username = XmppUsername;
            Xmpp.Password = XmppPassword;
            Xmpp.Priority = 1;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnAuthError += new XmppElementHandler(Xmpp_OnAuthError);
            Xmpp.OnStreamError += XmppOnOnStreamError;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
        }

        private static void XmppOnOnStreamError(object sender , Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
                Trace.WriteLine("[Bot]Someone Logged In As GameBot");
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

        static void Xmpp_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            Trace.WriteLine("[Bot]AuthError:" + e);
        }

        private static void XmppOnOnError(object sender , Exception exception)
        {
            Trace.WriteLine("[Bot]Error:" + exception.Message);
        }

        private static void XmppOnOnXmppConnectionStateChanged(object sender , XmppConnectionState state)
        {
            Trace.WriteLine("[Bot]ConState:" + state.ToString());
            if(state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private static bool isPaused = false;

        private static Queue<Message> messageQueue = new Queue<Message>();

        private static void XmppOnOnMessage(object sender , Message msg)
        {
            switch(msg.Type)
            {
                case MessageType.normal:
                    if(msg.Subject == "hostgame")
                    {
                        if (isPaused)
                        {
                            messageQueue.Enqueue(msg);
                            return;
                        }
                        var data = msg.Body.Split(new string[1]{",:,"},StringSplitOptions.RemoveEmptyEntries);
                        if (data.Length != 3) return;
                        var guid = Guid.Empty;
                        Version ver = null;
                        if (String.IsNullOrWhiteSpace(data[2])) return;
                        var gameName = data[2];
                        if(Guid.TryParse(data[0] , out guid) && Version.TryParse(data[1] , out ver))
                        {
                            var port = Gaming.HostGame(guid , ver , gameName , "" , new Lobby.User(msg.From));
                            if (port == -1) return;
                            var m = new Message(msg.From , msg.To , MessageType.normal , port.ToString() , "gameready");
                            m.GenerateId();
                            Xmpp.Send(m);
                        }
                    }
                    else if(msg.Subject == "gamelist")
                    {
                        if (isPaused)
                        {
                            messageQueue.Enqueue(msg);
                            return;
                        }
                        var list = Gaming.GetLobbyList().Where(x=>x.GameStatus == EHostedGame.StartedHosting);
                        var m = new Message(msg.From , MessageType.normal , "" , "gamelist");
                        m.GenerateId();
                        foreach (var a in list)
                        {
                            m.AddChild(a);
                        }
                        Xmpp.Send(m);
                    }
                    else if (msg.Subject == "gamestarted")
                    {
                        if (isPaused)
                        {
                            messageQueue.Enqueue(msg);
                            return;
                        }
                        int port = -1;
                        if (Int32.TryParse(msg.Body, out port)) Gaming.StartGame(port);
                    }
                    else
                    {
                        if (!msg.From.User.Equals("d0c", StringComparison.InvariantCultureIgnoreCase)) return;
                        if (msg.Body.Equals("pause"))
                        {
                            isPaused = true;
                            Console.WriteLine(":::::: PAUSED ::::::");
                            var m = new Message(msg.From, MessageType.chat, "Paused");
                            m.GenerateId();
                            Xmpp.Send(m);
                        }
                        else if (msg.Body.Equals("unpause"))
                        {
                            isPaused = false;
                            Console.WriteLine("Unpausing...");
                            var m = new Message(msg.From, MessageType.chat, "Unpausing");
                            m.GenerateId();
                            Xmpp.Send(m);
                            while (messageQueue.Count > 0)
                            {
                                XmppOnOnMessage(null,messageQueue.Dequeue());
                            }
                            Console.WriteLine(":::::: UNPAUSED ::::::");
                            var m2 = new Message(msg.From, MessageType.chat, "UnPaused");
                            m2.GenerateId();
                            Xmpp.Send(m2);
                        }
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
    }
}
