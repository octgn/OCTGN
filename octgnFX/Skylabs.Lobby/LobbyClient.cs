using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.GData.Client;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public enum LoginResult { Success, Failure, Banned };

    public enum DataRecType { FriendList, OnlineList, UserCustomStatus };

    public class LobbyClient : SkySocket
    {
        public delegate void LoginFinished(LoginResult success, DateTime banEnd, string message);
        public delegate void HandleCaptcha(string fullurl, string imageurl);
        public delegate void DataRecieved(DataRecType type, object e);
        public delegate void UserStatusChanged(UserStatus eve, User u);
        public delegate void FriendRequest(User u);
        public delegate void SocketMessageResult(SocketMessage sm);
        public delegate void GameHostEvent(HostedGame g);

        /// <summary>
        /// Kind of a generic event whenever data is received. Check DataRecType for data that triggers this.
        /// You can add more to handle other events as well.
        /// </summary>
        public event DataRecieved OnDataRecieved;
        /// <summary>
        /// This happens when there is a UserStatus change of any type, be it DisplayName, Status, or CustomStatus
        /// It's best to ignore UserStatus, and just pull the data from User.
        /// </summary>
        public event UserStatusChanged OnUserStatusChanged;
        /// <summary>
        /// Happens when we receive a friend request.
        /// </summary>
        public event FriendRequest OnFriendRequest;
        /// <summary>
        /// When google requires a Captcha, this gets called.
        /// </summary>
        public event HandleCaptcha OnCaptchaRequired;
        /// <summary>
        /// When a game has a hosting event, this gets called. The three are Game Hosting and ready for players, game in progress, and game done.
        /// </summary>
        public event GameHostEvent OnGameHostEvent;
        /// <summary>
        /// This happens when the LobbyClient disconnects for any reason.
        /// </summary>
        public event EventHandler OnDisconnectEvent;
        
        /// <summary>
        /// A list of Hosted games
        /// </summary>
        public List<HostedGame> Games { get; set; } 

        /// <summary>
        /// Meh, failed attempt for Asyncronus callbacks. Don't delete it, it gets used, but still.
        /// </summary>
        private Dictionary<string, SocketMessageResult> Callbacks;

        /// <summary>
        /// This handles all chatting stuff.
        /// </summary>
        public Chatting Chatting { get; set; }

        /// <summary>
        /// This is the current logged in user.
        /// </summary>
        public User Me { get; private set; }

        private LoginFinished _onLoginFinished;

        private string _mCaptchaToken = "";

        /// <summary>
        /// Assembly version of the LobbySoftware I think.
        /// </summary>
        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                return asm.GetName().Version;
            }
        }

        /// <summary>
        /// List of friends
        /// </summary>
        public List<User> FriendList { get; private set; }
        /// <summary>
        /// List of Notifications? I don't know offhand
        /// </summary>
        //TODO Figure out what this is for
        public List<Notification> Notifications { get; set; }
        /// <summary>
        /// Who knows
        /// </summary>
        public int CurrentHostedGamePort { get; set; }

        public LobbyClient()
        {
            FriendList = new List<User>();
            Notifications = new List<Notification>();
            Callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
            Chatting = new Lobby.Chatting(this);
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
            FriendList = new List<User>();
            Notifications = new List<Notification>();
            Callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
        }
        /// <summary>
        /// Start hosting a game.
        /// </summary>
        /// <param name="callback">Callback for when the server talks back</param>
        /// <param name="game">Game</param>
        /// <param name="gamename">Name of the game</param>
        /// <param name="password">Password</param>
        public void BeginHostGame(SocketMessageResult callback, Octgn.Data.Game game, string gamename, string password)
        {
            Callbacks.Add("hostgameresponse",callback);
            SocketMessage sm = new SocketMessage("hostgame");
            sm.AddData("game",game.Id);
            sm.AddData("version",game.Version);
            sm.AddData("name",gamename);
            sm.AddData("pass",password);
            WriteMessage(sm);
        }
        public User GetFriendFromUID(int uid)
        {
            foreach(User u in FriendList)
            {
                if (u.Uid == uid)
                    return u;
            }
            return null;
        }
        /// <summary>
        /// This gets called when the hoster of the game clicks 'Start game'
        /// </summary>
        public void HostedGameStarted()
        {
            if (CurrentHostedGamePort != -1)
            {
                SocketMessage sm = new SocketMessage("gamestarted");
                sm.AddData("port", CurrentHostedGamePort);
                WriteMessage(sm);
            }
        }
        /// <summary>
        /// Send a friend request to an e-mail
        /// </summary>
        /// <param name="email">E-mail of the friend</param>
        public void AddFriend(string email)
        {
            SocketMessage sm = new SocketMessage("addfriend");
            sm.AddData("email", email);
            WriteMessage(sm);
        }

        /// <summary>
        /// Login here
        /// </summary>
        /// <param name="onFinish">Delegate for when Login is done.</param>
        /// <param name="email">Users e-mail address</param>
        /// <param name="password">Password</param>
        /// <param name="captcha">Captcha string if required</param>
        /// <param name="status">Status to log in as</param>
        public void Login(LoginFinished onFinish, string email, string password, string captcha, UserStatus status)
        {
            if(Connected)
            {
                Thread t = new Thread(() =>
                                          {
                                              //TODO Need to add a method to handle 2-step signin.
                                              _onLoginFinished = onFinish;
                                              String appName = "skylabs-LobbyClient-" + Version;
                                              Service s = new Service("code", appName);
                                              s.setUserCredentials(email, password);
                                              if(captcha != null && _mCaptchaToken != null)
                                              {
                                                  if(!String.IsNullOrWhiteSpace(captcha) || !String.IsNullOrWhiteSpace(_mCaptchaToken))
                                                  {
                                                      s.Credentials.CaptchaToken = _mCaptchaToken;
                                                      s.Credentials.CaptchaAnswer = captcha;
                                                  }
                                              }
                                              try
                                              {
                                                  string ret = s.QueryClientLoginToken();
                                                  SocketMessage sm = new SocketMessage("login");
                                                  sm.AddData(new NameValuePair("email", email));
                                                  sm.AddData(new NameValuePair("token", ret));
                                                  sm.AddData("status", status);
                                                  WriteMessage(sm);
                                              }
                                              catch(CaptchaRequiredException ce)
                                              {
                                                  _mCaptchaToken = ce.Token;
                                                  if(OnCaptchaRequired != null) OnCaptchaRequired.Invoke("https://www.google.com/accounts/DisplayUnlockCaptcha", ce.Url);
                                              }
                                              catch(AuthenticationException re)
                                              {
                                                  string cu = (string)re.Data["CaptchaUrl"];
                                                  onFinish.Invoke(LoginResult.Failure, DateTime.Now, re.Message);
                                              }
                                              catch(WebException)
                                              {
                                                  onFinish.Invoke(LoginResult.Failure, DateTime.Now, "Connection problem.");
                                              }
                                          });
                t.Start();
            }
        }

        /// <summary>
        /// Whenever a SkySocket gets a message, it goes here for processing.
        /// </summary>
        /// <param name="sm">SocketMessage</param>
        public override void OnMessageReceived(Net.SocketMessage sm)
        {
            User u;
            if (Callbacks.ContainsKey(sm.Header.ToLower()))
            {
                SocketMessageResult a = Callbacks[sm.Header.ToLower()];
                if (a != null)
                {
                    a.Invoke(sm);
                    Callbacks.Remove(sm.Header.ToLower());
                    return;
                }
            }
            switch (sm.Header.ToLower())
            {
                case "end":
                    {
                        Close(DisconnectReason.CleanDisconnect);
                        break;
                    }
                case "loginsuccess":
                    Me = (User)sm["me"];
                    if (Me != null)
                    {
                        _onLoginFinished.Invoke(LoginResult.Success, DateTime.Now, "");
                        Chatting.JoinChatRoom(0);
                    }
                    else
                    {
                        _onLoginFinished.Invoke(LoginResult.Failure, DateTime.Now, "Data failure.");
                        Close(DisconnectReason.CleanDisconnect);
                    }
                    break;
                case "loginfailed":

                    _onLoginFinished.Invoke(LoginResult.Failure, DateTime.Now, (sm["message"] != null) ? (string)sm["message"] : "");
                    break;
                case "friends":
                    FriendList = new List<User>();
                    foreach (NameValuePair p in sm.Data)
                    {
                        FriendList.Add((User)p.Value);
                    }
                    if (OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.FriendList, null);
                    break;
                case "friendrequest":
                    u = (User)sm.Data[0].Value;
                    foreach (Notification n in Notifications)
                    {
                        FriendRequestNotification fr = n as FriendRequestNotification;
                        if (fr != null)
                        {
                            if (fr.User.Uid == u.Uid)
                                return;
                        }
                    }
                    Notifications.Add(new FriendRequestNotification(u, this));
                    if (OnFriendRequest != null)
                        OnFriendRequest(u);
                    break;
                case "status":
                    u = (User)sm.Data[0].Value;
                    User f = FriendList.FirstOrDefault(us => us.Equals(u));
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
                        OnUserStatusChanged.Invoke(u.Status, u);
                    if (OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.FriendList, null);
                    break;
                case "customstatus":
                    u = (User)sm["user"];
                    string s = (string)sm["status"];
                    if (u != null && s != null)
                    {
                        if (u.Equals(Me))
                            Me.CustomStatus = s;
                        else
                        {
                            int i = FriendList.IndexOf(u);
                            if (i > -1)
                                FriendList[i].CustomStatus = s;
                            this.OnDataRecieved(DataRecType.UserCustomStatus, u);
                        }
                    }
                    break;
                case "banned":
                    int time = (int)sm["end"];

                    _onLoginFinished.Invoke(LoginResult.Banned, Skylabs.ValueConverters.FromPhpTime(time), "");
                    break;
                case "gamelist":
                    {
                        List<HostedGame> games = sm["list"] as List<HostedGame>;
                        Games = games;
                        if (games.Count > 0)
                            if (OnGameHostEvent != null)
                                OnGameHostEvent.Invoke(Games[0]);
                        break;
                    }
                case "gamehosting":
                    {
                        HostedGame gm = new HostedGame(sm);
                        Games.Add(gm);
                        if (OnGameHostEvent != null)
                            OnGameHostEvent.Invoke(gm);
                        break;
                    }
                case "gamestarted":
                    {
                        int p = (int)sm["port"];

                        HostedGame gm = Games.FirstOrDefault(g => g.Port == p);
                        gm.GameStatus = HostedGame.eHostedGame.GameInProgress;
                        if (OnGameHostEvent != null)
                            OnGameHostEvent.Invoke(gm);
                        break;
                    }
                case "gameend":
                    {
                        int p = (int)sm["port"];

                        HostedGame gm = Games.Where(g => g.Port == p).First();
                        if (gm != null)
                        {
                            gm.GameStatus = HostedGame.eHostedGame.StoppedHosting;
                            if (OnGameHostEvent != null)
                                OnGameHostEvent.Invoke(gm);
                            Games.Remove(gm);
                        }
                        break;
                    }
                case "userjoinedchatroom":
                    {
                        User us = (User)sm["user"];
                        List<User> allusers = (List<User>)sm["allusers"];
                        long? id = (long?)sm["roomid"];
                        if (us == null || allusers == null || id == null)
                            return;
                        long id2 = (long)id;
                        Chatting.UserJoinedChat(id2, us, allusers);
                        break;
                    }
                case "userleftchatroom":
                    {
                        User us = (User)sm["user"];
                        long? id = (long?)sm["roomid"];
                        if (us == null || id == null)
                            return;
                        long id2 = (long)id;
                        Chatting.UserLeftChat(id2, us);
                        break;
                    }
                case "chatmessage":
                    {
                        User us = (User)sm["user"];
                        long? id = (long?)sm["roomid"];
                        string mess = (string)sm["mess"];
                        if (us == null || id == null || mess == null)
                            return;
                        long id2 = (long)id;
                        Chatting.RecieveChatMessage(id2, us, mess);
                        break;
                    }

            }
        }
        /// <summary>
        /// Sets the users status. Don't ever set to Offline, use Invisible instead.
        /// </summary>
        /// <param name="s">Users status</param>
        public void SetStatus(UserStatus s)
        {
            SocketMessage sm = new SocketMessage("status");
            sm.AddData("status", s);
            WriteMessage(sm);
            Me.Status = s;
        }
        /// <summary>
        /// Sets the users custom status.
        /// </summary>
        /// <param name="CustomStatus"></param>
        public void SetCustomStatus(string CustomStatus)
        {
            SocketMessage sm = new SocketMessage("customstatus");
            sm.AddData("customstatus", CustomStatus);
            WriteMessage(sm);
        }
        /// <summary>
        /// Sets the users display name
        /// </summary>
        /// <param name="name"></param>
        public void SetDisplayName(string name)
        {
            SocketMessage sm = new SocketMessage("displayname");
            sm.AddData("name", name);
            WriteMessage(sm);
        }
        /// <summary>
        /// Happens when the SkySocket disconnects.
        /// </summary>
        /// <param name="reason"></param>
        public override void OnDisconnect(Net.DisconnectReason reason)
        {
            if (OnDisconnectEvent != null)
                OnDisconnectEvent(this, null);
        }
    }
}