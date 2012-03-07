using System;
using System.Collections.Generic;
using System.Linq;
using Skylabs.Lobby;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class ChatRoom : IComparable<ChatRoom>, IEquatable<ChatRoom>
    {
        private readonly object _userLocker = new object();

        /// <summary>
        ///   List of users in the chat room.8
        /// </summary>
        private List<Pair<int, User>> _users = new List<Pair<int, User>>();

        /// <summary>
        ///   initializes a chat room, and adds the initial user. This should only be called by Chatting.cs
        /// </summary>
        /// <param name="id"> ID for the room. </param>
        /// <param name="initialUser"> User making the room </param>
        public ChatRoom(long id, User initialUser)
        {
            Id = id;
            if (initialUser != null)
                AddUser(initialUser);
        }

        /// <summary>
        ///   Unique ID of the chat room
        /// </summary>
        public long Id { get; private set; }

        #region IComparable<ChatRoom> Members

        /// <summary>
        ///   Compare this ChatRoom to the other room. Based on the ChatRoom ID
        /// </summary>
        /// <param name="other"> Other chat room </param>
        /// <returns> General compare integers </returns>
        public int CompareTo(ChatRoom other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion

        #region IEquatable<ChatRoom> Members

        /// <summary>
        ///   Is this ChatRoom.ID == to Other.ID?
        /// </summary>
        /// <param name="other"> other room </param>
        /// <returns> true if equal, false if not. </returns>
        public bool Equals(ChatRoom other)
        {
            return Id == other.Id;
        }

        #endregion

        /// <summary>
        ///   Add a user to the room
        /// </summary>
        /// <param name="u"> User to add </param>
        /// <returns> Returns true on success, or false if there was an explosion </returns>
        public bool AddUser(User u)
        {
            lock (_userLocker)
            {
                Client c = Server.GetOnlineClientByUid(u.Uid);
                if (c != null)
                {
                    var sm = new SocketMessage("userjoinedchatroom");
                    sm.AddData("roomid", Id);
                    sm.AddData("user", u);
                    List<User> ulist = _users.Select(p => p.Item2).ToList();
                    Pair<int, User> ou = _users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                    if (ou == null)
                    {
                        _users.Add(new Pair<int, User>(1, u));
                        ulist.Add(u);
                        sm.AddData("allusers", ulist);
                        SendAllUsersMessage(sm, false);
                    }
                    else
                    {
                        ou.Item1++;
                        sm.AddData("allusers", ulist);
                        c.WriteMessage(sm);
                        return true;
                    }
                }
                return false;
            }
        }

        public User[] GetUserList()
        {
            lock (_userLocker)
            {
                var ret = new User[_users.Count];
                for (int i = 0; i < _users.Count; i++)
                {
                    ret[i] = _users[i].Item2;
                }
                return ret;
            }
        }

        /// <summary>
        ///   Chatting.cs calls this when a user exits, this doesn't need to be called ever again.
        /// </summary>
        /// <param name="u"> The user. </param>
        public void UserExit(User u)
        {
            lock (_userLocker)
            {
                Pair<int, User> ou = _users.FirstOrDefault(us => us.Item2.Uid == u.Uid);
                if (ou == null) return;
                ou.Item1--;
                if (ou.Item1 != 0) return;
                _users.Remove(ou);
                var sm = new SocketMessage("userleftchatroom");
                sm.AddData("roomid", Id);
                sm.AddData("user", u);
                SendAllUsersMessage(sm, false);
            }
        }

        /// <summary>
        ///   Sends all users in this chat room a message
        /// </summary>
        /// <param name="sm"> Message to send </param>
        /// <param name="Lock"> Should we lock? </param>
        private void SendAllUsersMessage(SocketMessage sm, bool Lock)
        {
            if (Lock)
            {
                lock (_userLocker)
                {
                    var slist = new int[_users.Count];
                    int i = 0;
                    foreach (Pair<int, User> u in _users)
                    {
                        slist[i] = u.Item2.Uid;
                        i++;
                    }
                    LazyAsync.Invoke(() => Server.AllUserMessageUidList(slist, sm));
                }
            }
            else
            {
                var slist = new int[_users.Count];
                int i = 0;
                foreach (Pair<int, User> u in _users)
                {
                    slist[i] = u.Item2.Uid;
                    i++;
                }
                LazyAsync.Invoke(() => Server.AllUserMessageUidList(slist, sm));
            }
        }

        /// <summary>
        ///   Chatting.cs calls this when a chat message is received. Shouldn't need to be called ever again.
        /// </summary>
        /// <param name="u"> User from </param>
        /// <param name="message"> The message </param>
        public void ChatMessage(User u, String message)
        {
            var sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", Id);
            sm.AddData("mess", message);
            sm.AddData("user", u);
            SendAllUsersMessage(sm, false);
        }
    }
}