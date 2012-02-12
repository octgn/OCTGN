using System;
using System.Collections.Generic;
using System.Linq;
using Skylabs.Lobby;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Chatting
    {
        static Chatting()
        {
            Rooms = new List<ChatRoom> {new ChatRoom(0, null)};
        }

        /// <summary>
        ///   List of all the open ChatRooms
        /// </summary>
        private static List<ChatRoom> Rooms { get; set; }

        /// <summary>
        ///   Join a chat room.
        /// </summary>
        /// <param name="c"> Client that wants to join </param>
        /// <param name="s"> Socket message stuffed with data. </param>
        public static void JoinChatRoom(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
                var rid = (long?) s["roomid"];
                if (rid == null)
                    return;
                if (rid == -1)
                    MakeRoom(c);
                else
                {
                    var cr = Rooms.FirstOrDefault(r => r.Id == rid);
                    if (cr != null)
                        cr.AddUser(c.Me);
                    else
                        MakeRoom(c);
                }
            }
        }

        /// <summary>
        ///   This starts up a two person chat.
        /// </summary>
        /// <param name="c"> Client starting the room </param>
        /// <param name="s"> Socket message full of data </param>
        public static void TwoPersonChat(Client c, SocketMessage s)
        {
            var user = (User) s["user"];
            if (user == null)
                return;
            lock (Rooms)
            {
                if (
                    Rooms.Select(cr => cr.GetUserList()).Any(
                        ul => ul.Contains(user) && ul.Contains(c.Me) && ul.Length == 2))
                {
                    return;
                }
                var id = MakeRoom(c);
                s.AddData("roomid", id);
            }
            AddUserToChat(c, s);
        }

        /// <summary>
        ///   Add a user to a chat room
        /// </summary>
        /// <param name="c"> Client adding a user </param>
        /// <param name="s"> Socket message with the user data and stuff </param>
        public static void AddUserToChat(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
                var rid = (long?)s["roomid"];
                if (rid == null || rid == -1)
                {
                    return;
                }
                var user = (User)s["user"];
                if (user == null)
                {
                    return;
                }
                var cr = Rooms.FirstOrDefault(r => r.Id == rid);
                if (cr != null)
                {
                    cr.AddUser(user);
                }
            }
        }

        /// <summary>
        ///   Make a room, hosted by Client
        /// </summary>
        /// <param name="c"> Client "hosting" </param>
        /// <returns> The unique chat room id. </returns>
        private static long MakeRoom(Client c)
        {
            var newId = DateTime.Now.Ticks;
            while (Rooms.Count(r => r.Id == newId) != 0)
            {
                newId = DateTime.Now.Ticks;
            }
            Rooms.Add(new ChatRoom(newId, c.Me));
            return newId;
        }

        /// <summary>
        ///   Removes a chat room from the list.
        /// </summary>
        /// <param name="room"> Chat room to remove </param>
        public static void RemoveRoom(ChatRoom room)
        {
            lock (Rooms)
            {
                Rooms.Remove(room);
            }
        }

        /// <summary>
        ///   Server calls this when a User goes offline.
        /// </summary>
        /// <param name="u"> User </param>
        public static void UserOffline(User u)
        {
            lock (Rooms)
            {
                var roomstocan = new List<long>();
                foreach (var c in Rooms)
                {
                    var cr = c;
                    LazyAsync.Invoke(() => cr.UserExit(u));
                    var ul = c.GetUserList();
                    if (ul.Length - 1 <= 0 && c.Id != 0)
                        roomstocan.Add(c.Id);
                }
                foreach (var l in roomstocan)
                {
                    Rooms.RemoveAll(r => r.Id == l);
                }
            }
        }

        /// <summary>
        ///   Gets called when a user expressely leaves a chat room.
        /// </summary>
        /// <param name="u"> User </param>
        /// <param name="rid"> Room id </param>
        public static void UserLeaves(User u, long rid)
        {
            lock (Rooms)
            {
                var cr = Rooms.FirstOrDefault(r => r.Id == rid);
                if (cr == null) return;
                LazyAsync.Invoke(() => cr.UserExit(u));
                var ul = cr.GetUserList();
                if (ul.Length - 1 <= 0 && cr.Id != 0)
                    Rooms.RemoveAll(r => r.Id == cr.Id);
            }
        }

        /// <summary>
        ///   Whenever a chat message request comes in, it goes through here and gets filtered to the appropriate room.
        /// </summary>
        /// <param name="c"> Client as the sender </param>
        /// <param name="sm"> Data as the data. </param>
        public static void ChatMessage(Client c, SocketMessage sm)
        {
            lock (Rooms)
            {
                var rid = (long?) sm["roomid"];
                var mess = (string) sm["mess"];
                if (rid == null || mess == null)
                {
                    return;
                }
                if (String.IsNullOrWhiteSpace(mess))
                {
                    return;
                }
                var rid2 = (long) rid;
                var cr = Rooms.FirstOrDefault(r => r.Id == rid2);
                if (cr != null)
                {
                    LazyAsync.Invoke(() => cr.ChatMessage(c.Me, mess));
                }
            }
        }
    }
}