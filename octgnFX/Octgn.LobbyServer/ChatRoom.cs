using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class ChatRoom : IComparable<ChatRoom>,IEquatable<ChatRoom>
    {
        public long ID { get; private set; }
        public List<User> Users { get; private set; }
        public ChatRoom(long id, User initialUser)
        {
            Users = new List<User>();
            lock(Users)
            {
                ID = id;
                if(initialUser != null)
                    AddUser(initialUser);
            }
        }
        public bool AddUser(User u)
        {
            lock(Users)
            {
                if(!Users.Exists(us => us.Uid == u.Uid))
                {
                    if (Program.Server.GetOnlineClientByUid(u.Uid) != null)
                    {
                        Users.Add(u);
                        SocketMessage sm = new SocketMessage("userjoinedchatroom");
                        sm.AddData("roomid", ID);
                        sm.AddData("user", u);
                        sm.AddData("allusers", Users);
                        SendAllUsersMessage(sm);
                        return true;
                    }
                }
                return false;
            }
        }
        public void UserExit(User u)
        {
            lock (Users)
            {
                if (Users.Exists(us => us.Uid == u.Uid))
                {
                    Users.Remove(u);
                    SocketMessage sm = new SocketMessage("userleftchatroom");
                    sm.AddData("roomid", ID);
                    sm.AddData("user", u);
                    SendAllUsersMessage(sm);
                }
            }
        }
        public void SendAllUsersMessage(SocketMessage sm)
        {
            foreach (User u in Users)
            {
                Client c = Program.Server.GetOnlineClientByUid(u.Uid);
                c.WriteMessage(sm);
            }
        }
        public void ChatMessage(User u, String message)
        {
            SocketMessage sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", ID);
            sm.AddData("mess", message);
            sm.AddData("user", u);
            SendAllUsersMessage(sm);
        }
        public int CompareTo(ChatRoom other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(ChatRoom other)
        {
            return ID == other.ID;
        }
    }
}
