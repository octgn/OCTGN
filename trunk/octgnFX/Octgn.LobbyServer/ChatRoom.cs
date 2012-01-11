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
        /// <summary>
        /// Unique ID of the chat room
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// List of users in the chat room.8
        /// </summary>
        private List<User> Users = new List<User>();

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
            lock (UserLocker)
            {
                Console.WriteLine("LOCK(AddUser)UserLocker");
                if (!Users.Exists(us => us.Uid == u.Uid))
                {
                    if (Server.GetOnlineClientByUid(u.Uid) != null)
                    {
                        Users.Add(u);
                        SocketMessage sm = new SocketMessage("userjoinedchatroom");
                        sm.AddData("roomid", ID);
                        sm.AddData("user", u);
                        sm.AddData("allusers", Users);
                        SendAllUsersMessage(sm,false);
                        Console.WriteLine("UNLOCK(AddUser)UserLocker");
                        return true;
                    }
                }
                Console.WriteLine("UNLOCK(AddUser)UserLocker");
                return false;
            }
        }
        public User[] GetUserList()
        {
            lock(UserLocker)
            {
                Console.WriteLine("LOCK(GetUserList)UserLocker");
                User[] ret =  Users.ToArray();
                Console.WriteLine("UNLOCK(GetUserList)UserLocker");
                return ret;
            }
        }
        /// <summary>
        /// Chatting.cs calls this when a user exits, this doesn't need to be called ever again.
        /// </summary>
        /// <param name="u">The user.</param>
        public void UserExit(User u)
        {
            lock (UserLocker)
            {
                Console.WriteLine("LOCK(UserExit)UserLocker");
                if (Users.Exists(us => us.Uid == u.Uid))
                {
                    Users.Remove(u);
                    SocketMessage sm = new SocketMessage("userleftchatroom");
                    sm.AddData("roomid", ID);
                    sm.AddData("user", u);
                    SendAllUsersMessage(sm,false);
                }
                Console.WriteLine("UNLOCK(UserExit)UserLocker");
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
                lock(UserLocker)
                {
                    Console.WriteLine("LOCK(SendAllUsersMessage)UserLocker");
                    foreach (User u in Users)
                    {
                        Client c = Server.GetOnlineClientByUid(u.Uid);
                        c.WriteMessage(sm);
                    }
                }
                Console.WriteLine("UNLOCK(SendAllUsersMessage)UserLocker");
            }
            else
            {
                foreach (User u in Users)
                {
                    Client c = Server.GetOnlineClientByUid(u.Uid);
                    c.WriteMessage(sm);
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
            SendAllUsersMessage(sm,true);
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
