using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Skylabs.Lobby;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class ChatRoom : IComparable<ChatRoom>, IEquatable<ChatRoom>
    {
        private readonly object UserLocker = new object();

        /// <summary>
        /// List of users in the chat room.8
        /// </summary>
        private readonly List<Pair<int, User>> Users = new List<Pair<int, User>>();

        /// <summary>
        /// initializes a chat room, and adds the initial user.
        /// This should only be called by Chatting.cs
        /// </summary>
        /// <param name="id">ID for the room.</param>
        /// <param name="initialUser">User making the room</param>
        public ChatRoom(long id, User initialUser)
        {
            ID = id;
            if (initialUser != null)
                AddUser(initialUser);
        }

        /// <summary>
        /// Unique ID of the chat room
        /// </summary>
        public long ID { get; private set; }

        #region IComparable<ChatRoom> Members

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

        #endregion

        #region IEquatable<ChatRoom> Members

        /// <summary>
        /// Is this ChatRoom.ID == to Other.ID?
        /// </summary>
        /// <param name="other">other room</param>
        /// <returns>true if equal, false if not.</returns>
        public bool Equals(ChatRoom other)
        {
            return ID == other.ID;
        }

        #endregion

        /// <summary>
        /// Add a user to the room
        /// </summary>
        /// <param name="u">User to add</param>
        /// <returns>Returns true on success, or false if there was an explosion</returns>
        public bool AddUser(User u)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "UserLocker");
            lock (UserLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "UserLocker");
                Client c = Server.GetOnlineClientByUid(u.Uid);
                if (c != null)
                {
                    var sm = new SocketMessage("userjoinedchatroom");
                    sm.AddData("roomid", ID);
                    sm.AddData("user", u);
                    var ulist = new List<User>();
                    foreach (var p in Users)
                        ulist.Add(p.Item2);
                    Pair<int, User> ou = Users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                    if (ou == null)
                    {
                        Users.Add(new Pair<int, User>(1, u));
                        ulist.Add(u);
                        sm.AddData("allusers", ulist);
                        SendAllUsersMessage(sm, false);
                    }
                    else
                    {
                        ou.Item1++;
                        sm.AddData("allusers", ulist);
                        c.WriteMessage(sm);
                        Logger.UL(MethodBase.GetCurrentMethod().Name, "UserLocker");
                        return true;
                    }
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "UserLocker");
                return false;
            }
        }

        public User[] GetUserList()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "UserLocker");
            lock (UserLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "UserLocker");
                var ret = new User[Users.Count];
                for (int i = 0; i < Users.Count; i++)
                {
                    ret[i] = Users[i].Item2;
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "UserLocker");
                return ret;
            }
        }

        /// <summary>
        /// Chatting.cs calls this when a user exits, this doesn't need to be called ever again.
        /// </summary>
        /// <param name="u">The user.</param>
        public void UserExit(User u)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "UserLocker");
            lock (UserLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "UserLocker");
                Pair<int, User> ou = Users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                if (ou != null)
                {
                    ou.Item1--;
                    if (ou.Item1 == 0)
                    {
                        Users.Remove(ou);
                        var sm = new SocketMessage("userleftchatroom");
                        sm.AddData("roomid", ID);
                        sm.AddData("user", u);
                        SendAllUsersMessage(sm, false);
                    }
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "UserLocker");
            }
        }

        /// <summary>
        /// Sends all users in this chat room a message
        /// </summary>
        /// <param name="sm">Message to send</param>
        private void SendAllUsersMessage(SocketMessage sm, bool Lock)
        {
            if (Lock)
            {
                Logger.TL(MethodBase.GetCurrentMethod().Name, "UserLocker");
                lock (UserLocker)
                {
                    Logger.L(MethodBase.GetCurrentMethod().Name, "UserLocker");
                    var slist = new int[Users.Count];
                    int i = 0;
                    foreach (var u in Users)
                    {
                        slist[i] = u.Item2.Uid;
                        i++;
                    }
                    LazyAsync.Invoke(() => Server.AllUserMessageUidList(slist, sm));
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "UserLocker");
            }
            else
            {
                var slist = new int[Users.Count];
                int i = 0;
                foreach (var u in Users)
                {
                    slist[i] = u.Item2.Uid;
                    i++;
                }
                LazyAsync.Invoke(() => Server.AllUserMessageUidList(slist, sm));
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
            var sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", ID);
            sm.AddData("mess", message);
            sm.AddData("user", u);
            SendAllUsersMessage(sm, false);
        }
    }
}