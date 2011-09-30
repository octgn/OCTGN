using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Google.GData.Client;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public enum LoginResult { Success, Failure, Banned };

    public enum DataRecType { FriendList, OnlineList, UserCustomStatus };

    public class LobbyClient : SkySocket
    {
        public delegate void LoginFinished(LoginResult success, DateTime BanEnd, string message);
        public delegate void HandleCaptcha(string Fullurl, string Imageurl);
        public delegate void DataRecieved(DataRecType type, object e);
        public delegate void UserStatusChanged(Skylabs.Lobby.UserStatus eve, User u);
        public delegate void FriendRequest(User u);
        public event DataRecieved OnDataRecieved;
        public event UserStatusChanged OnUserStatusChanged;
        public event FriendRequest OnFriendRequest;
        public event HandleCaptcha OnCaptchaRequired;

        public User Me { get; private set; }

        private LoginFinished OnLoginFinished;

        private string m_Email = "";
        private string m_Password = "";
        private string m_CaptchaToken = "";

        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                AssemblyProductAttribute at = (AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
                return asm.GetName().Version;
            }
        }

        public List<User> FriendList { get; private set; }

        public List<User> OnlineList { get; private set; }

        public LobbyClient()
            : base()
        {
            FriendList = new List<User>();
            OnlineList = new List<User>();
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
            FriendList = new List<User>();
            OnlineList = new List<User>();
        }

        public User GetOnlineUser(int uid)
        {
            foreach(User u in OnlineList)
            {
                if(u.UID == uid) return u;
            }
            return null;
        }

        public void Add_Friend(string email)
        {
            SocketMessage sm = new SocketMessage("addfriend");
            sm.Add_Data("email", email);
            WriteMessage(sm);
        }

        public void Login(LoginFinished onFinish, string email, string password, string captcha, UserStatus status)
        {
            if(Connected)
            {
                Thread t = new Thread(new ThreadStart(() =>
                {
                    OnLoginFinished = onFinish;
                    this.m_Email = email;
                    this.m_Password = password;
                    String AppName = "skylabs-LobbyClient-" + Version.ToString();
                    Service s = new Service("code", AppName);
                    s.setUserCredentials(email, password);
                    if(captcha != null && m_CaptchaToken != null)
                    {
                        if(!String.IsNullOrWhiteSpace(captcha) || !String.IsNullOrWhiteSpace(m_CaptchaToken))
                        {
                            s.Credentials.CaptchaToken = m_CaptchaToken;
                            s.Credentials.CaptchaAnswer = captcha;
                        }
                    }
                    try
                    {
                        string ret = s.QueryClientLoginToken();
                        SocketMessage sm = new SocketMessage("login");
                        sm.Add_Data(new NameValuePair("email", email));
                        sm.Add_Data(new NameValuePair("token", ret));
                        sm.Add_Data("status", status);
                        WriteMessage(sm);
                    }
                    catch(Google.GData.Client.CaptchaRequiredException ce)
                    {
                        m_CaptchaToken = ce.Token;
                        if(OnCaptchaRequired != null) OnCaptchaRequired.Invoke("https://www.google.com/accounts/DisplayUnlockCaptcha", ce.Url);
                    }
                    catch(Google.GData.Client.AuthenticationException re)
                    {
                        string cu = (string)re.Data["CaptchaUrl"];
                        onFinish.Invoke(LoginResult.Failure, DateTime.Now, re.Message);
                    }
                    catch(WebException e)
                    {
                        onFinish.Invoke(LoginResult.Failure, DateTime.Now, "Connection problem.");
                    }
                }));
                t.Start();
            }
        }

        public void Accept_Friend_Request(int uid, bool accept)
        {
            SocketMessage sm = new SocketMessage("acceptfriend");
            sm.Add_Data("uid", uid);
            sm.Add_Data("accept", accept);
            WriteMessage(sm);
        }

        public override void OnMessageReceived(Net.SocketMessage sm)
        {
            User u;
            switch(sm.Header.ToLower())
            {
                case "loginsuccess":
                    Me = (User)sm["me"];
                    if(Me != null)
                        OnLoginFinished.Invoke(LoginResult.Success, DateTime.Now, "");
                    else
                    {
                        OnLoginFinished.Invoke(LoginResult.Failure, DateTime.Now, "Data failure.");
                        this.Close(DisconnectReason.CleanDisconnect);
                    }
                    break;
                case "loginfailed":

                    OnLoginFinished.Invoke(LoginResult.Failure, DateTime.Now, (sm["message"] != null) ? (string)sm["message"] : "");
                    break;
                case "friends":
                    FriendList = new List<User>();
                    foreach(NameValuePair p in sm.Data)
                    {
                        FriendList.Add((User)p.Value);
                    }
                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.FriendList, null);
                    break;
                case "friendrequest":
                    u = (User)sm.Data[0].Value;
                    if(OnFriendRequest != null)
                        OnFriendRequest(u);
                    break;
                case "onlinelist":
                    OnlineList = new List<User>();
                    foreach(NameValuePair p in sm.Data)
                        OnlineList.Add((User)p.Value);
                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.OnlineList, null);
                    break;
                case "status":
                    u = (User)sm.Data[0].Value;
                    if(!OnlineList.Contains(u)) OnlineList.Add(u);
                    if(OnUserStatusChanged != null)
                        OnUserStatusChanged.Invoke(u.Status, u);
                    break;
                case "customstatus":
                    u = (User)sm["user"];
                    string s = (string)sm["status"];
                    if(u != null && s != null)
                    {
                        if(u.Equals(Me))
                            Me.CustomStatus = s;
                        else
                        {
                            int i = FriendList.IndexOf(u);
                            if(i > -1)
                                FriendList[i].CustomStatus = s;
                            i = OnlineList.IndexOf(u);
                            if(i > -1)
                                OnlineList[i].CustomStatus = s;
                            this.OnDataRecieved(DataRecType.UserCustomStatus, u);
                        }
                    }
                    break;
                case "banned":
                    int time = (int)sm["end"];

                    OnLoginFinished.Invoke(LoginResult.Banned, Skylabs.ValueConverters.fromPHPTime(time), "");
                    break;
            }
        }

        public void SetStatus(UserStatus s)
        {
            SocketMessage sm = new SocketMessage("status");
            sm.Add_Data("status", s);
            WriteMessage(sm);
            Me.Status = s;
        }

        public void SetCustomStatus(string CustomStatus)
        {
            SocketMessage sm = new SocketMessage("customstatus");
            sm.Add_Data("customstatus", CustomStatus);
            WriteMessage(sm);
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}