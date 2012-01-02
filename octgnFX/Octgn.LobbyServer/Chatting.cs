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
        public static List<ChatRoom> Rooms { get; set; }
        static Chatting()
        {
            Rooms = new List<ChatRoom>();
            Rooms.Add(new ChatRoom(0, null));
        }
        public static void JoinChatRoom(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
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
            }
        }
        public static void TwoPersonChat(Client c, SocketMessage s)
        {
            var user = (User)s["user"];
            if (user == null)
                return;
            lock(Rooms)
            {
                foreach (ChatRoom cr in Rooms)
                {
                    if (cr.Users.Contains(user) && cr.Users.Contains(c.Me) && cr.Users.Count == 2)
                        return;
                }
            }
            long id = MakeRoom(c);
            s.AddData("roomid", id);
            AddUserToChat(c, s);
        }
        public static void AddUserToChat(Client c, SocketMessage s)
        {
            lock (Rooms)
            {
                var rid = (long?)s["roomid"];
                if (rid == null || rid == -1)
                    return;
                var user = (User)s["user"];
                if (user == null)
                    return;
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    if (cr.AddUser(user))
                    {
                        return;
                    }
                }
            }
        }
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
        public static void RemoveRoom(ChatRoom room)
        {
            Rooms.Remove(room);
        }
        public static void UserOffline(User u)
        {
            lock (Rooms)
            {
                List<long> roomstocan = new List<long>();
                foreach (ChatRoom c in Rooms)
                {
                    c.UserExit(u);
                    if (c.Users.Count == 0 && c.ID != 0)
                        roomstocan.Add(c.ID);
                }
                foreach (long l in roomstocan)
                {
                    Rooms.RemoveAll(r => r.ID == l);
                }
            }
        }
        public static void UserLeaves(User u, long rid)
        {
            lock (Rooms)
            {
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                    cr.UserExit(u);
            }
        }
        public static void ChatMessage(Client c,SocketMessage sm)
        {
            lock (Rooms)
            {
                long? rid = (long?)sm["roomid"];
                string mess = (string)sm["mess"];
                if (rid == null || mess == null)
                    return;
                long rid2 = (long)rid;
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid2);
                if (cr != null)
                    cr.ChatMessage(c.Me, mess);
            }
        }
    }
}
