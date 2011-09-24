using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public enum LoginResult { Success, Failure, Banned };

    public enum DataRecType { FriendList, OnlineList };

    public enum UserStatus { Online, Offline }

    public class LobbyClient : SkySocket
    {
        public delegate void LoginFinished(LoginResult success, DateTime BanEnd);
        public delegate void RegisterFinished(string emailerror, string passworderror, string usernameerror);
        public delegate void DataRecieved(DataRecType type);
        public delegate void UserStatusChanged(UserStatus eve, User u);
        public delegate void FriendRequest(User u);
        public event DataRecieved OnDataRecieved;
        public event UserStatusChanged OnUserStatusChanged;
        public event FriendRequest OnFriendRequest;
        private LoginFinished OnLoginFinished;
        private RegisterFinished OnRegisterFinished;

        public List<int> FriendList { get; private set; }

        public List<User> OnlineList { get; private set; }

        public LobbyClient()
            : base()
        {
            FriendList = new List<int>();
            OnlineList = new List<User>();
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
            FriendList = new List<int>();
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

        public void Login(LoginFinished onFinish, string email, string password)
        {
            if(Connected)
            {
                OnLoginFinished = onFinish;
                SocketMessage sm = new SocketMessage("login");
                sm.Add_Data(new NameValuePair("email", email));
                sm.Add_Data(new NameValuePair("password", password));
                WriteMessage(sm);
            }
        }

        public void Register(RegisterFinished onFinish, string email, string password, string username)
        {
            if(Connected)
            {
                OnRegisterFinished = onFinish;
                SocketMessage sm = new SocketMessage("register");
                sm.Add_Data(new NameValuePair("email", email));
                sm.Add_Data(new NameValuePair("password", password));
                sm.Add_Data(new NameValuePair("username", username));
                WriteMessage(sm);
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
                    OnLoginFinished.Invoke(LoginResult.Success, DateTime.Now);
                    break;
                case "loginfailed":
                    OnLoginFinished.Invoke(LoginResult.Failure, DateTime.Now);
                    break;
                case "friends":
                    FriendList = new List<int>();
                    foreach(NameValuePair p in sm.Data)
                    {
                        FriendList.Add((int)p.Value);
                    }
                    if(OnDataRecieved != null)
                        OnDataRecieved.Invoke(DataRecType.FriendList);
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
                        OnDataRecieved.Invoke(DataRecType.OnlineList);
                    break;
                case "useronline":
                    u = (User)sm.Data[0].Value;
                    if(!OnlineList.Contains(u)) OnlineList.Add(u);
                    if(OnUserStatusChanged != null)
                        OnUserStatusChanged.Invoke(UserStatus.Online, u);
                    break;
                case "useroffline":
                    User temp = new User();
                    temp.UID = (int)sm.Data[0].Value;
                    u = (User)OnlineList[OnlineList.IndexOf(temp)].Clone();
                    OnlineList.Remove(u);
                    if(OnUserStatusChanged != null)
                        OnUserStatusChanged.Invoke(UserStatus.Offline, u);
                    break;
                case "banned":
                    int time = (int)sm["end"];

                    OnLoginFinished.Invoke(LoginResult.Banned, Skylabs.ValueConverters.fromPHPTime(time));
                    break;
                case "registersuccess":
                    OnRegisterFinished(null, null, null);
                    break;
                case "registerfailed":
                    string email = (string)sm["email"];
                    string password = (string)sm["password"];
                    string username = (string)sm["username"];
                    OnRegisterFinished(email, password, username);
                    break;
            }
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}