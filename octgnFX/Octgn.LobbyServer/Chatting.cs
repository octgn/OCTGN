using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Net;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public static class Chatting
    {
        /// <summary>
        /// List of all the open ChatRooms
        /// </summary>
        private static List<ChatRoom> Rooms { get; set; }
        static Chatting()
        {
            Rooms = new List<ChatRoom>();
            Rooms.Add(new ChatRoom(0, null));
        }
        /// <summary>
        /// Join a chat room.
        /// </summary>
        /// <param name="c">Client that wants to join</param>
        /// <param name="s">Socket message stuffed with data.</param>
        public static void JoinChatRoom(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(JoinChatRoom)Rooms");
                var rid = (long?)s["roomid"];
                if (rid == null)
                    return;
                if (rid == -1)
                    MakeRoom(c);
                else
                {
                    ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                    if (cr != null)
                        cr.AddUser(c.Me);
                    else
                        MakeRoom(c);
                }
                Console.WriteLine("UNLOCK(JoinChatRoom)Rooms");
            }
        }
        /// <summary>
        /// This starts up a two person chat.
        /// </summary>
        /// <param name="c">Client starting the room</param>
        /// <param name="s">Socket message full of data</param>
        public static void TwoPersonChat(Client c, SocketMessage s)
        {
            var user = (User)s["user"];
            if (user == null)
                return;
            lock(Rooms)
            {
                Console.WriteLine("LOCK(TwoPersonChat)Rooms");
                foreach (ChatRoom cr in Rooms)
                {
                    User[] ul = cr.GetUserList();
                    if (ul.Contains(user) && ul.Contains(c.Me) && ul.Length == 2)
                    {
                        Console.WriteLine("UNLOCK(TwoPersonChat)Rooms");
                        return;
                    }
                }
            }
            Console.WriteLine("UNLOCK(TwoPersonChat)Rooms");
            long id = MakeRoom(c);
            s.AddData("roomid", id);
            AddUserToChat(c, s);
        }
        /// <summary>
        /// Add a user to a chat room
        /// </summary>
        /// <param name="c">Client adding a user</param>
        /// <param name="s">Socket message with the user data and stuff</param>
        public static void AddUserToChat(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(AddUserToChat)Rooms");
                var rid = (long?)s["roomid"];
                if (rid == null || rid == -1)
                {
                    Console.WriteLine("UNLOCK(AddUserToChat)Rooms");
                    return;
                }
                var user = (User)s["user"];
                if (user == null)
                {
                    Console.WriteLine("UNLOCK(AddUserToChat)Rooms");
                    return;
                }
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    if (cr.AddUser(user))
                    {
                        Console.WriteLine("UNLOCK(AddUserToChat)Rooms");
                        return;
                    }
                }
            }
            Console.WriteLine("UNLOCK(AddUserToChat)Rooms");
        }
        /// <summary>
        /// Make a room, hosted by Client
        /// </summary>
        /// <param name="c">Client "hosting"</param>
        /// <returns>The unique chat room id.</returns>
        private static long MakeRoom(Client c)
        {
            long newID = DateTime.Now.Ticks;
            while (Rooms.Count(r => r.ID == newID) != 0)
            {
                newID = DateTime.Now.Ticks;
            }
            Rooms.Add(new ChatRoom(newID, c.Me));
            return newID;
        }
        /// <summary>
        /// Removes a chat room from the list.
        /// </summary>
        /// <param name="room">Chat room to remove</param>
        public static void RemoveRoom(ChatRoom room)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(RemoveRoom)Rooms");
                Rooms.Remove(room);
                Console.WriteLine("UNLOCK(RemoveRoom)Rooms");
            }
        }
        /// <summary>
        /// Server calls this when a User goes offline.
        /// </summary>
        /// <param name="u">User</param>
        public static void UserOffline(User u)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(UserOffline)Rooms");
                List<long> roomstocan = new List<long>();
                foreach (ChatRoom c in Rooms)
                {
                    c.UserExit(u);
                    User[] ul = c.GetUserList();
                    if (ul.Length == 0 && c.ID != 0)
                        roomstocan.Add(c.ID);
                }
                foreach (long l in roomstocan)
                {
                    Rooms.RemoveAll(r => r.ID == l);
                }
                Console.WriteLine("UNLOCK(UserOffline)Rooms");
            }
        }
        /// <summary>
        /// Gets called when a user expressely leaves a chat room.
        /// </summary>
        /// <param name="u">User</param>
        /// <param name="rid">Room id</param>
        public static void UserLeaves(User u, long rid)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(UserLeaves)Rooms");
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                    cr.UserExit(u);
                Console.WriteLine("UNLOCK(UserLeaves)Rooms");
            }
        }
        /// <summary>
        /// Whenever a chat message request comes in, it goes through here
        /// and gets filtered to the appropriate room.
        /// </summary>
        /// <param name="c">Client as the sender</param>
        /// <param name="sm">Data as the data.</param>
        public static void ChatMessage(Client c,SocketMessage sm)
        {
            lock (Rooms)
            {
                Console.WriteLine("LOCK(ChatMessage)Rooms");
                long? rid = (long?)sm["roomid"];
                string mess = (string)sm["mess"];
                if (rid == null || mess == null)
                {
                    Console.WriteLine("UNLOCK(ChatMessage)Rooms");
                    return;
                }
                if (String.IsNullOrWhiteSpace(mess))
                {
                    Console.WriteLine("UNLOCK(ChatMessage)Rooms");
                    return;
                }
                long rid2 = (long)rid;
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid2);
                if (cr != null)
                    cr.ChatMessage(c.Me, mess);
                Console.WriteLine("UNLOCK(ChatMessage)Rooms");
            }
        }
    }
}
