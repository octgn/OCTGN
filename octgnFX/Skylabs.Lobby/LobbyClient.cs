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
        public event DataRecieved OnDataRecieved;
        public event UserStatusChanged OnUserStatusChanged;
        public event FriendRequest OnFriendRequest;
        public event HandleCaptcha OnCaptchaRequired;
        public event GameHostEvent OnGameHostEvent;
        

        public List<HostedGame> Games { get; set; } 

        private Dictionary<string, SocketMessageResult> Callbacks; 



        public User Me { get; private set; }

        private LoginFinished _onLoginFinished;

        private string _mCaptchaToken = "";

        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                return asm.GetName().Version;
            }
        }

        public List<User> FriendList { get; private set; }

        public List<User> OnlineList { get; private set; }

        public List<Notification> Notifications { get; set; }

        public int CurrentHostedGamePort { get; set; }

        public LobbyClient()
        {
            FriendList = new List<User>();
            OnlineList = new List<User>();
            Notifications = new List<Notification>();
            Callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
            FriendList = new List<User>();
            OnlineList = new List<User>();
            Notifications = new List<Notification>();
            Callbacks = new Dictionary<string, SocketMessageResult>();
            Games = new List<HostedGame>();
        }

        public User GetOnlineUser(int uid)
        {
            return OnlineList.FirstOrDefault(u => u.Uid == uid);
        }

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
        public void HostedGameStarted()
        {
            if (CurrentHostedGamePort != -1)
            {
                SocketMessage sm = new SocketMessage("gamestarted");
                sm.AddData("port", CurrentHostedGamePort);
                WriteMessage(sm);
            }
        }
        public void AddFriend(string email)
        {
            SocketMessage sm = new SocketMessage("addfriend");
            sm.AddData("email", email);
            WriteMessage(sm);
        }

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
                case "loginsuccess":
                    Me = (User)sm["me"];
                    if (Me != null)
                        _onLoginFinished.Invoke(LoginResult.Success, DateTime.Now, "");
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
                    Notifications.Add(new FriendRequestNotification(u, this));
                    if (OnFriendRequest != null)
                        OnFriendRequest(u);
                    break;
                case "onlinelist":
                    OnlineList = new List<User>();
                    foreach (NameValuePair p in sm.Data)
                        OnlineList.Add((User)p.Value);
                    if (OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.OnlineList, null);
                    break;
                case "status":
                    u = (User)sm.Data[0].Value;
                    if (u.Status == UserStatus.Offline)
                        OnlineList.Remove(u);
                    else if (!OnlineList.Contains(u))
                        OnlineList.Add(u);
                    else
                        OnlineList.Where(us => us.Equals(u)).First().Status = u.Status;

                    User f = FriendList.FirstOrDefault(us => us.Equals(u));
                    if (f != null)
                    {
                        f.Status = u.Status;
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
                            i = OnlineList.IndexOf(u);
                            if (i > -1)
                                OnlineList[i].CustomStatus = s;
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
                        int p = (int) sm["port"];

                        HostedGame gm = Games.FirstOrDefault(g => g.Port == p);
                        gm.GameStatus = HostedGame.eHostedGame.GameInProgress;
                        if(OnGameHostEvent != null)
                            OnGameHostEvent.Invoke(gm);
                        break;
                    }
                case "gameend":
                    {
                        int p = (int) sm["port"];

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

            }
        }

        public void SetStatus(UserStatus s)
        {
            SocketMessage sm = new SocketMessage("status");
            sm.AddData("status", s);
            WriteMessage(sm);
            Me.Status = s;
        }

        public void SetCustomStatus(string CustomStatus)
        {
            SocketMessage sm = new SocketMessage("customstatus");
            sm.AddData("customstatus", CustomStatus);
            WriteMessage(sm);
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}