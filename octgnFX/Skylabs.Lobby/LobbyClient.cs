using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Google.GData.Client;
using Octgn.Data;
using Skylabs.Lobby.Sockets;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.Lobby
{
    public enum LoginResult
    {
        Success,
        Failure,
        Banned,
        WaitingForResponse
    };

    public enum DataRecType
    {
        FriendList,
        OnlineList,
        UserCustomStatus,
        ServerMessage
    };

    public class LobbyClient
    {
        #region Delegates

        public delegate void DataRecieved(DataRecType type, object e);

        public delegate void FriendRequest(User u);

        public delegate void GameHostEvent(HostedGame g);

        public delegate void HandleCaptcha(string fullurl, string imageurl);

        public delegate void LoginFinished(LoginResult success, DateTime banEnd, string message);

        public delegate void LoginProgressUpdate(string message);

        public delegate void SocketMessageResult(SocketMessage sm);

        public delegate void UserStatusChanged(UserStatus eve, User u);

        #endregion

        /// <summary>
        ///   Meh, failed attempt for Asyncronus callbacks. Don't delete it, it gets used, but still.
        /// </summary>
        private readonly Dictionary<string, SocketMessageResult> _callbacks;

        private readonly object _friendLocker = new object();

        private readonly object _gameLocker = new object();
        private readonly object _noteLocker = new object();
        private readonly SkySocket _socket;

        /// <summary>
        ///   Assembly version of the LobbySoftware I think.
        /// </summary>
        public Version Version = Assembly.GetCallingAssembly().GetName().Version;

        private bool _didCallStop;
        private string _mCaptchaToken = "";
        private int _nextNoteId;
        private LoginFinished _onLoginFinished;

        public LobbyClient()
        {
            FriendList = new List<User>();
            Notifications = new List<Notification>();
            _callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
            Chatting = new Chatting(this);
            _socket = new SkySocket();
            _socket.OnMessageReceived += OnMessageReceived;
            _socket.OnConnectionClosed += Socket_OnConnectionClosed;
        }

        public LobbyClient(SkySocket c)
        {
            FriendList = new List<User>();
            Notifications = new List<Notification>();
            _callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
            _socket = c;
        }

        /// <summary>
        ///   A list of Hosted games
        /// </summary>
        private List<HostedGame> Games { get; set; }

        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        /// <summary>
        ///   This handles all chatting stuff.
        /// </summary>
        public Chatting Chatting { get; set; }

        /// <summary>
        ///   This is the current logged in user.
        /// </summary>
        public User Me { get; private set; }

        /// <summary>
        ///   List of friends
        /// </summary>
        private List<User> FriendList { get; set; }

        /// <summary>
        ///   List of Notifications? I don't know offhand
        /// </summary>
        //TODO Figure out what this is for
        private List<Notification> Notifications { get; set; }

        /// <summary>
        ///   Who knows
        /// </summary>
        public int CurrentHostedGamePort { get; set; }

        public event EventHandler OnDisconnect;

        /// <summary>
        ///   Kind of a generic event whenever data is received. Check DataRecType for data that triggers this. You can add more to handle other events as well.
        /// </summary>
        public event DataRecieved OnDataRecieved;

        /// <summary>
        ///   This happens when there is a UserStatus change of any type, be it DisplayName, Status, or CustomStatus It's best to ignore UserStatus, and just pull the data from User.
        /// </summary>
        public event UserStatusChanged OnUserStatusChanged;

        /// <summary>
        ///   Happens when we receive a friend request.
        /// </summary>
        public event FriendRequest OnFriendRequest;

        /// <summary>
        ///   When google requires a Captcha, this gets called.
        /// </summary>
        public event HandleCaptcha OnCaptchaRequired;

        /// <summary>
        ///   When a game has a hosting event, this gets called. The three are Game Hosting and ready for players, game in progress, and game done.
        /// </summary>
        public event GameHostEvent OnGameHostEvent;

        // private bool _sentEndMessage; // not used right now

        public bool Connect(string host, int port)
        {
            return _socket.Connect(host, port);
        }

        private void Socket_OnConnectionClosed(SkySocket socket)
        {
            if (OnDisconnect != null)
            {
                OnDisconnect.BeginInvoke(null, null, null, null);
            }
            _socket.Dispose();
        }

        /// <summary>
        ///   Disconnect cleanly
        /// </summary>
        public void Stop()
        {
            if (_didCallStop) return;
            _didCallStop = true;
            WriteMessage(new SocketMessage("end"));
            _socket.Stop();
        }

        public void WriteMessage(SocketMessage sm)
        {
            _socket.WriteMessage(sm);
        }

        /// <summary>
        ///   Start hosting a game.
        /// </summary>
        /// <param name="callback"> Callback for when the server talks back </param>
        /// <param name="game"> Game </param>
        /// <param name="gamename"> Name of the game </param>
        /// <param name="password"> Password </param>
        public void BeginHostGame(SocketMessageResult callback, Game game, string gamename, string password)
        {
            _callbacks.Clear();
            _callbacks.Add("hostgameresponse", callback);
            var sm = new SocketMessage("hostgame");
            sm.AddData("game", game.Id);
            sm.AddData("version", game.Version);
            sm.AddData("name", gamename);
            sm.AddData("pass", password);
            WriteMessage(sm);
        }

        public Notification[] GetNotificationList()
        {
            lock (_noteLocker)
            {
                return Notifications.ToArray();
            }
        }

        public void RemoveNotification(Notification note)
        {
            lock (_noteLocker)
            {
                Notifications.Remove(note);
            }
        }

        public HostedGame[] GetHostedGames()
        {
            lock (_gameLocker)
            {
                return Games.ToArray();
            }
        }

        public User[] GetFriendsList()
        {
            lock (_friendLocker)
            {
                return FriendList.ToArray();
            }
        }

        public User GetFriendFromUid(int uid)
        {
            lock (_friendLocker)
            {
                return FriendList.FirstOrDefault(u => u.Uid == uid);
            }
        }

        /// <summary>
        ///   This gets called when the hoster of the game clicks 'Start game'
        /// </summary>
        public void HostedGameStarted()
        {
            if (CurrentHostedGamePort == -1) return;
            var sm = new SocketMessage("gamestarted");
            sm.AddData("port", CurrentHostedGamePort);
            WriteMessage(sm);
        }

        /// <summary>
        ///   Send a friend request to an e-mail
        /// </summary>
        /// <param name="email"> E-mail of the friend </param>
        public void AddFriend(string email)
        {
            var sm = new SocketMessage("addfriend");
            sm.AddData("email", email);
            WriteMessage(sm);
        }

        /// <summary>
        ///   Login here
        /// </summary>
        /// <param name="onFinish"> Delegate for when Login is done. </param>
        /// <param name="onUpdate"> </param>
        /// <param name="email"> Users e-mail address </param>
        /// <param name="password"> Password </param>
        /// <param name="captcha"> Captcha string if required </param>
        /// <param name="status"> Status to log in as </param>
        public void Login(LoginFinished onFinish, LoginProgressUpdate onUpdate, string email, string password,
                          string captcha, UserStatus status)
        {
            if (!_socket.Connected) return;
            var t = new Thread(() =>
                                   {
                                       //TODO Need to add a method to handle 2-step signin.
                                       _onLoginFinished = onFinish;
                                       var appName = "skylabs-LobbyClient-" + Version;
                                       var s = new Service("code", appName);
                                       s.setUserCredentials(email, password);
                                       if (captcha != null && _mCaptchaToken != null)
                                       {
                                           onUpdate.Invoke("Verifying captcha");
                                           if (!String.IsNullOrWhiteSpace(captcha) ||
                                               !String.IsNullOrWhiteSpace(_mCaptchaToken))
                                           {
                                               s.Credentials.CaptchaToken = _mCaptchaToken;
                                               s.Credentials.CaptchaAnswer = captcha;
                                           }
                                       }
                                       try
                                       {
                                           Debug.WriteLine("Querying Google...");
                                           onUpdate.Invoke("Logging into Google...");
                                           var ret = s.QueryClientLoginToken();
                                           onUpdate.Invoke("Sending login token to Server...");
                                           Debug.WriteLine("Received login token.");
                                           var sm = new SocketMessage("login");
                                           sm.AddData("email", email);
                                           sm.AddData("token", ret);
                                           sm.AddData("status", status);
                                           WriteMessage(sm);
                                           onUpdate.Invoke("Waiting for server response...");
                                       }
                                       catch (CaptchaRequiredException ce)
                                       {
                                           _mCaptchaToken = ce.Token;
                                           if (OnCaptchaRequired != null)
                                               OnCaptchaRequired.Invoke(
                                                   "https://www.google.com/accounts/DisplayUnlockCaptcha", ce.Url);
                                       }
                                       catch (AuthenticationException re)
                                       {
                                           // var cu = (string) re.Data["CaptchaUrl"]; // unused
                                           onFinish.Invoke(LoginResult.Failure, DateTime.Now, re.Message);
                                       }
                                       catch (WebException)
                                       {
                                           onFinish.Invoke(LoginResult.Failure, DateTime.Now, "Connection problem.");
                                       }
                                       onFinish.Invoke(LoginResult.WaitingForResponse, DateTime.Now, "");
                                   });
            t.Start();
        }

        /// <summary>
        ///   Whenever a SkySocket gets a message, it goes here for processing.
        /// </summary>
        /// <param name="ss"> SkySocket </param>
        /// <param name="sm"> SocketMessage </param>
        private void OnMessageReceived(SkySocket ss, SocketMessage sm)
        {
            User u;
            if (_callbacks.ContainsKey(sm.Header.ToLower()))
            {
                var a = _callbacks[sm.Header.ToLower()];
                if (a != null)
                {
                    a.Invoke(sm);
                    _callbacks.Remove(sm.Header.ToLower());
                    return;
                }
            }
            switch (sm.Header.ToLower())
            {
                case "end":
                    {
                        Stop();
                        break;
                    }
                case "loginsuccess":
                    Trace.TraceInformation("Got LoginSuccess");
                    Me = (User) sm["me"];
                    if (Me != null)
                    {
                        _onLoginFinished.Invoke(LoginResult.Success, DateTime.Now, "");
                        Chatting.JoinChatRoom(0);
                    }
                    else
                    {
                        _onLoginFinished.Invoke(LoginResult.Failure, DateTime.Now, "Data failure.");
                        Stop();
                        //Close(DisconnectReason.CleanDisconnect);
                    }
                    break;
                case "loginfailed":
                    Trace.TraceInformation("Got LoginFailed");
                    _onLoginFinished.Invoke(LoginResult.Failure, DateTime.Now,
                                            (sm["message"] != null) ? (string) sm["message"] : "");
                    break;
                case "friends":
                    lock (_friendLocker)
                    {
                        FriendList = new List<User>();
                        foreach (var p in sm.Data)
                        {
                            FriendList.Add((User) p.Value);
                        }
                        if (OnDataRecieved != null)
                        {
                            foreach (DataRecieved d in OnDataRecieved.GetInvocationList())
                                d.BeginInvoke(DataRecType.FriendList, null, r => { }, null);
                        }
                    }
                    break;
                case "servermessage":
                    {
                        var mess = sm["message"] as string;
                        if (mess != null && OnDataRecieved != null)
                        {
                            foreach (DataRecieved d in OnDataRecieved.GetInvocationList())
                                d.BeginInvoke(DataRecType.ServerMessage, mess, r => { }, null);
                        }
                        break;
                    }
                case "friendrequest":
                    lock (_noteLocker)
                    {
                        u = (User) sm.Data[0].Value;
                        if (Notifications.OfType<FriendRequestNotification>().Any(fr => fr.User.Uid == u.Uid))
                        {
                            return;
                        }
                        Notifications.Add(new FriendRequestNotification(u, this, _nextNoteId));
                        _nextNoteId++;
                        if (OnFriendRequest != null)
                            foreach (FriendRequest fr in OnFriendRequest.GetInvocationList())
                                fr.BeginInvoke(u, r => { }, null);
                    }
                    break;
                case "status":
                    lock (_friendLocker)
                    {
                        u = (User) sm.Data[0].Value;
                        var f = FriendList.FirstOrDefault(us => us.Equals(u));
                        if (f != null)
                        {
                            f.DisplayName = u.DisplayName;
                            f.Status = u.Status;
                            f.CustomStatus = u.CustomStatus;
                        }
                        if (u.Equals(Me))
                        {
                            Me.DisplayName = u.DisplayName;
                            Me.Status = u.Status;
                            Me.CustomStatus = u.CustomStatus;
                        }
                        if (OnUserStatusChanged != null)
                            foreach (UserStatusChanged usc in OnUserStatusChanged.GetInvocationList())
                                usc.BeginInvoke(u.Status, u, r => { }, null);
                        if (OnDataRecieved != null)
                            foreach (DataRecieved dr in OnDataRecieved.GetInvocationList())
                                dr.BeginInvoke(DataRecType.FriendList, null, r => { }, null);
                    }
                    break;
                case "customstatus":
                    lock (_friendLocker)
                    {
                        u = (User) sm["user"];
                        var s = (string) sm["status"];
                        if (u != null && s != null)
                        {
                            if (u.Equals(Me))
                                Me.CustomStatus = s;
                            else
                            {
                                var i = FriendList.IndexOf(u);
                                if (i > -1)
                                    FriendList[i].CustomStatus = s;
                                if (OnDataRecieved != null)
                                    foreach (DataRecieved dr in OnDataRecieved.GetInvocationList())
                                        dr.BeginInvoke(DataRecType.UserCustomStatus, u, r => { }, null);
                            }
                        }
                    }
                    break;
                case "banned":
                    var time = (int) sm["end"];

                    _onLoginFinished.Invoke(LoginResult.Banned, ValueConverters.FromPhpTime(time), "");
                    break;
                case "gamelist":
                    {
                        lock (_gameLocker)
                        {
                            var games = sm["list"] as List<HostedGame>;
                            Games = games;
                            if (games != null && games.Count > 0)
                                if (OnGameHostEvent != null)
                                    foreach (GameHostEvent ge in OnGameHostEvent.GetInvocationList())
                                        ge.BeginInvoke(Games[0], r => { }, null);
                        }
                        break;
                    }
                case "gamehosting":
                    {
                        lock (_gameLocker)
                        {
                            var gm = new HostedGame(sm);
                            Games.Add(gm);
                            if (OnGameHostEvent != null)
                                LazyAsync.Invoke(() => OnGameHostEvent.Invoke(gm));
                        }
                        break;
                    }
                case "gamestarted":
                    {
                        lock (_gameLocker)
                        {
                            var p = (int) sm["port"];

                            var gm = Games.FirstOrDefault(g => g.Port == p);
                            if (gm != null)
                            {
                                gm.GameStatus = HostedGame.EHostedGame.GameInProgress;
                                if (OnGameHostEvent != null)
                                    LazyAsync.Invoke(() => OnGameHostEvent.Invoke(gm));
                            }
                        }
                        break;
                    }
                case "gameend":
                    {
                        lock (_gameLocker)
                        {
                            var p = (int) sm["port"];

                            var gm = Games.FirstOrDefault(g => g.Port == p);
                            if (gm != null)
                            {
                                gm.GameStatus = HostedGame.EHostedGame.StoppedHosting;
                                if (OnGameHostEvent != null)
                                    LazyAsync.Invoke(() => OnGameHostEvent.Invoke(gm));
                                Games.Remove(gm);
                            }
                        }
                        break;
                    }
                case "userjoinedchatroom":
                    {
                        var us = (User) sm["user"];
                        var allusers = (List<User>) sm["allusers"];
                        var id = (long?) sm["roomid"];
                        if (us == null || allusers == null || id == null)
                            return;
                        var id2 = (long) id;
                        Chatting.UserJoinedChat(id2, us, allusers);
                        break;
                    }
                case "userleftchatroom":
                    {
                        var us = (User) sm["user"];
                        var id = (long?) sm["roomid"];
                        if (us == null || id == null)
                            return;
                        var id2 = (long) id;
                        Chatting.UserLeftChat(id2, us);
                        break;
                    }
                case "chatmessage":
                    {
                        var us = (User) sm["user"];
                        var id = (long?) sm["roomid"];
                        var mess = (string) sm["mess"];
                        if (us == null || id == null || mess == null)
                            return;
                        var id2 = (long) id;
                        Chatting.RecieveChatMessage(id2, us, mess);
                        break;
                    }
            }
        }

        /// <summary>
        ///   Sets the users status. Don't ever set to Offline, use Invisible instead.
        /// </summary>
        /// <param name="s"> Users status </param>
        public void SetStatus(UserStatus s)
        {
            var sm = new SocketMessage("status");
            sm.AddData("status", s);
            WriteMessage(sm);
            Me.Status = s;
        }

        /// <summary>
        ///   Sets the users custom status.
        /// </summary>
        /// <param name="customStatus"> </param>
        public void SetCustomStatus(string customStatus)
        {
            var sm = new SocketMessage("customstatus");
            sm.AddData("customstatus", customStatus);
            WriteMessage(sm);
        }

        /// <summary>
        ///   Sets the users display name
        /// </summary>
        /// <param name="name"> </param>
        public void SetDisplayName(string name)
        {
            var sm = new SocketMessage("displayname");
            sm.AddData("name", name);
            WriteMessage(sm);
        }
    }
}