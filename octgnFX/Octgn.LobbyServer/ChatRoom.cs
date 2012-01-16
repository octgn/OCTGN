using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using Skylabs.Net;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public class ChatRoom : IComparable<ChatRoom>,IEquatable<ChatRoom>
    {
        /// <summary>
        /// Unique ID of the chat room
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// List of users in the chat room.8
        /// </summary>
        private List<Pair<int, User>> Users = new List<Pair<int, User>>();

        private object UserLocker = new object();
        /// <summary>
        /// initializes a chat room, and adds the initial user.
        /// This should only be called by Chatting.cs
        /// </summary>
        /// <param name="id">ID for the room.</param>
        /// <param name="initialUser">User making the room</param>
        public ChatRoom(long id, User initialUser)
        {
            ID = id;
            if(initialUser != null)
                AddUser(initialUser);
        }
        /// <summary>
        /// Add a user to the room
        /// </summary>
        /// <param name="u">User to add</param>
        /// <returns>Returns true on success, or false if there was an explosion</returns>
        public bool AddUser(User u)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
            lock (UserLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "Adding User " + u.Uid.ToString() + " to room " + ID.ToString());
                Client c = Server.GetOnlineClientByUid(u.Uid);
                if (c != null)
                {
                    SocketMessage sm = new SocketMessage("userjoinedchatroom");
                    sm.AddData("roomid", ID);
                    sm.AddData("user", u);
                    List<User> ulist = new List<User>();
                    foreach (Pair<int, User> p in Users)
                        ulist.Add(p.Item2);
                    Pair<int, User> ou = Users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                    if (ou == null)
                    {
                        Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "User not in room already. Adding.");
                        Users.Add(new Pair<int, User>(1, u));
                        ulist.Add(u);
                        sm.AddData("allusers", ulist);
                        SendAllUsersMessage(sm, false);
                    }
                    else
                    {
                        ou.Item1++;
                        Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "User found in room. Incrementing to " + ou.Item1.ToString());
                        sm.AddData("allusers", ulist);
                        c.WriteMessage(sm);
                        Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                        return true;
                    }
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                return false;
            }
        }
        public User[] GetUserList()
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
            lock(UserLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                User[] ret = new User[Users.Count];
                for (int i = 0; i < Users.Count; i++)
                {
                    ret[i] = Users[i].Item2;
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                return ret;
            }
        }
        /// <summary>
        /// Chatting.cs calls this when a user exits, this doesn't need to be called ever again.
        /// </summary>
        /// <param name="u">The user.</param>
        public void UserExit(User u)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
            lock (UserLocker)
            {
                Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "User " + u.Uid.ToString() + " Exiting room " + ID.ToString());
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                Pair<int, User> ou = Users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                if (ou != null)
                {
                    ou.Item1--;
                    Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "User room decremented to " + ou.Item1.ToString());
                    if (ou.Item1 == 0)
                    {
                        Logger.log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "Removing user " + u.Uid.ToString() + " from room " + ID.ToString());
                        Users.Remove(ou);
                        SocketMessage sm = new SocketMessage("userleftchatroom");
                        sm.AddData("roomid", ID);
                        sm.AddData("user", u);
                        SendAllUsersMessage(sm, false);
                    }
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
            }
        }
        /// <summary>
        /// Sends all users in this chat room a message
        /// </summary>
        /// <param name="sm">Message to send</param>
        private void SendAllUsersMessage(SocketMessage sm,bool Lock)
        {
            if (Lock)
            {
                Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                lock(UserLocker)
                {
                    Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
                    foreach (Pair<int, User> u in Users)
                    {
                        Client c = Server.GetOnlineClientByUid(u.Item2.Uid);
                        if (c != null)
                        {
                            Thread t = new Thread(() => c.WriteMessage(sm));
                            t.Start();
                        }
                    }
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "UserLocker");
            }
            else
            {
                foreach (Pair<int, User> u in Users)
                {
                    Client c = Server.GetOnlineClientByUid(u.Item2.Uid);
                    if (c != null)
                    {
                        Thread t = new Thread(()=>c.WriteMessage(sm));
                        t.Start();
                    }
                }
            }
        }
        /// <summary>
        /// Chatting.cs calls this when a chat message is received. 
        /// Shouldn't need to be called ever again.
        /// </summary>
        /// <param name="u">User from</param>
        /// <param name="message">The message</param>
        public void ChatMessage(User u, String message)
        {
            SocketMessage sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", ID);
            sm.AddData("mess", message);
            sm.AddData("user", u);
            SendAllUsersMessage(sm,false);
        }
        /// <summary>
        /// Compare this ChatRoom to the other room.
        /// Based on the ChatRoom ID
        /// </summary>
        /// <param name="other">Other chat room</param>
        /// <returns>General compare integers</returns>
        public int CompareTo(ChatRoom other)
        {
            return ID.CompareTo(other.ID);
        }
        /// <summary>
        /// Is this ChatRoom.ID == to Other.ID?
        /// </summary>
        /// <param name="other">other room</param>
        /// <returns>true if equal, false if not.</returns>
        public bool Equals(ChatRoom other)
        {
            return ID == other.ID;
        }
    }
}
