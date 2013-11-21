using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using agsXMPP;
using agsXMPP.Factory;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using log4net;
using Octgn.Library;
using Skylabs.Lobby;

namespace Octgn.Online.GameService
{
    public class GameBot : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static GameBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static GameBot Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameBotSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private XmppClientConnection Xmpp { get; set; }

        public ObjectHandler OnCheckRecieved { get; set; }

        public GameBot()
        {
            RemakeXmpp();
        }
        public void RemakeXmpp()
        {
            if (Xmpp != null)
            {
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
                Xmpp.Close();
                Xmpp = null;
            }
            Xmpp = new XmppClientConnection(AppConfig.Instance.ServerPath);
            ElementFactory.AddElementType("hostgamerequest", "octgn:hostgamerequest", typeof(HostGameRequest));

            Xmpp.RegisterAccount = false;
            Xmpp.AutoAgents = true;
            Xmpp.AutoPresence = true;
            Xmpp.AutoRoster = true;
            Xmpp.Username = AppConfig.Instance.XmppUsername;
            Xmpp.Password = AppConfig.Instance.XmppPassword;
            Xmpp.Priority = 1;
            Xmpp.OnMessage += XmppOnOnMessage;
            Xmpp.OnError += XmppOnOnError;
            Xmpp.OnAuthError += Xmpp_OnAuthError;
            Xmpp.OnStreamError += XmppOnOnStreamError;
            Xmpp.KeepAlive = true;
            Xmpp.KeepAliveInterval = 60;
            Xmpp.OnAgentStart += XmppOnOnAgentStart;
            Xmpp.OnXmppConnectionStateChanged += XmppOnOnXmppConnectionStateChanged;
            Xmpp.Open();
        }

        private void XmppOnOnStreamError(object sender , Element element)
        {
            var textTag = element.GetTag("text");
            if (!String.IsNullOrWhiteSpace(textTag) && textTag.Trim().ToLower() == "replaced by new connection")
                Log.Error("Someone Logged In As GameBot");
        }

        private void XmppOnOnAgentStart(object sender) 
        {
            if(OnCheckRecieved != null)
                OnCheckRecieved.Invoke(sender);
        }

        public void CheckBotStatus()
        {
            Xmpp.RequestAgents();
        }

        void Xmpp_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            Log.ErrorFormat("AuthError: {0}",e);
        }

        private void XmppOnOnError(object sender , Exception exception)
        {
            Log.Error("[Bot]Error", exception);
        }

        private void XmppOnOnXmppConnectionStateChanged(object sender , XmppConnectionState state)
        {
            Log.InfoFormat("[Bot]Connection State Changed To {0}",state);
            if(state == XmppConnectionState.Disconnected)
                RemakeXmpp();
        }

        private bool _isPaused = false;

        private readonly Queue<Message> _messageQueue = new Queue<Message>();

        private void XmppOnOnMessage(object sender , Message msg)
        {
            switch(msg.Type)
            {
                case MessageType.normal:
                    if(msg.Subject == "hostgame")
                    {
                        if (_isPaused)
                        {
                            _messageQueue.Enqueue(msg);
                            return;
                        }

                        if (msg.HasChildElements == false)
                        {
                            // F it, someone screwed up this year.
                            return;
                        }

                        if (msg.ChildNodes.OfType<HostGameRequest>().Any() == false)
                        {
                            // Again, what the fuuuuu
                            return;
                        }

                        var req = msg.ChildNodes.OfType<HostGameRequest>().First();

                        GameManager.Instance.HostGame(req,new User(msg.From));

                        // let the host game send back a message when it's all done and shit.

                        //var data = msg.Body.Split(new string[1]{",:,"},StringSplitOptions.None);
                        //if (data.Length != 5) return;
                        //var guid = Guid.Empty;
                        //Version ver = null;
                        //if (String.IsNullOrWhiteSpace(data[2])) return;
                        //var gameName = data[2];
                        //var password = data[3];
                        //var gameActualName = data[4];
                        //if(Guid.TryParse(data[0] , out guid) && Version.TryParse(data[1] , out ver))
                        //{
                        //    var port = Gaming.HostGame(guid, ver, gameName, password, new Skylabs.Lobby.User(msg.From), gameActualName);
                        //    if (port == -1) return;
                        //    var m = new Message(msg.From , msg.To , MessageType.normal , port.ToString() , "gameready");
                        //    m.GenerateId();
                        //    Xmpp.Send(m);
                        //}
                    }
                    else if(msg.Subject == "gamelist")
                    {
                        if (_isPaused)
                        {
                            return;
                        }
                        var list = GameManager.Instance.Games.Where(x => x.GameStatus == EHostedGame.StartedHosting);
                        //var list = Gaming.GetLobbyList().Where(x=>x.GameStatus == EHostedGame.StartedHosting);
                        var m = new Message(msg.From , MessageType.normal , "" , "gamelist");
                        m.GenerateId();
                        foreach (var a in list)
                        {
                            m.AddChild(a);
                        }
                        Xmpp.Send(m);
                    }
                    // Don't need this one, because we'll get this data from the udp nonsense.
                    //else if (msg.Subject == "gamestarted")
                    //{
                    //    if (_isPaused)
                    //    {
                    //        _messageQueue.Enqueue(msg);
                    //        return;
                    //    }
                    //    int port = -1;
                    //    if (Int32.TryParse(msg.Body, out port)) Gaming.StartGame(port);
                    //}
                    break;
                case MessageType.error:
                    break;
                case MessageType.chat:
                        if (!msg.From.User.Equals("d0c", StringComparison.InvariantCultureIgnoreCase)) return;
                        if (msg.Body.Equals("pause"))
                        {
                            _isPaused = true;
                            Log.Warn(":::::: PAUSED ::::::");
                            var m = new Message(msg.From, MessageType.chat, "Paused");
                            m.GenerateId();
                            Xmpp.Send(m);
                        }
                        else if (msg.Body.Equals("unpause"))
                        {
                            _isPaused = false;
                            Log.Warn(":::::: UNPAUSING ::::::");
                            var m = new Message(msg.From, MessageType.chat, "Unpausing");
                            m.GenerateId();
                            Xmpp.Send(m);
                            while (_messageQueue.Count > 0)
                            {
                                XmppOnOnMessage(null,_messageQueue.Dequeue());
                            }
                            Log.Warn(":::::: UNPAUSED ::::::");
                            var m2 = new Message(msg.From, MessageType.chat, "UnPaused");
                            m2.GenerateId();
                            Xmpp.Send(m2);
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

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Xmpp != null)
            {
                Xmpp.OnMessage -= XmppOnOnMessage;
                Xmpp.OnError -= XmppOnOnError;
                Xmpp.OnAuthError -= Xmpp_OnAuthError;
                Xmpp.OnStreamError -= XmppOnOnStreamError;
                Xmpp.OnAgentStart -= XmppOnOnAgentStart;
                Xmpp.OnXmppConnectionStateChanged -= XmppOnOnXmppConnectionStateChanged;
            }
        }

        #endregion
    }
}
