using System.Collections.Generic;
using System.Linq;
using Skylabs.Net;

namespace Skylabs.Lobby
{
    public class Chatting
    {
        #region Delegates

        public delegate void ChatEventDelegate(ChatRoom cr, ChatEvent e, User u, object data);

        #endregion

        #region ChatEvent enum

        public enum ChatEvent
        {
            UserJoinedChat,
            UserLeftChat,
            MeJoinedChat,
            ChatMessage
        };

        #endregion

        private readonly LobbyClient Parent;

        public Chatting(LobbyClient c)
        {
            Parent = c;
            Rooms = new List<ChatRoom>();
            Rooms.Add(new ChatRoom(0));
        }

        public List<ChatRoom> Rooms { get; private set; }
        public event ChatEventDelegate eChatEvent;

        public ChatRoom GetChatRoomFromRID(long rid)
        {
            lock (Rooms)
            {
                foreach (ChatRoom r in Rooms)
                {
                    if (r.ID == rid)
                        return r;
                }
                return null;
            }
        }

        /// <summary>
        /// Create a chat room with another user.
        /// </summary>
        /// <param name="otherUser"></param>
        public void CreateChatRoom(User otherUser)
        {
            var sm = new SocketMessage("twopersonchat");
            sm.AddData("user", otherUser);
            Parent.WriteMessage(sm);
        }

        /// <summary>
        /// Join a chat room based on an RID
        /// </summary>
        /// <param name="id"></param>
        public void JoinChatRoom(long id)
        {
            var sm = new SocketMessage("joinchatroom");
            sm.AddData("roomid", id);
            Parent.WriteMessage(sm);
        }

        /// <summary>
        /// Add a user to a chat room
        /// </summary>
        /// <param name="u"></param>
        /// <param name="chatid"></param>
        public void AddUserToChat(User u, long chatid)
        {
            var sm = new SocketMessage("addusertochat");
            sm.AddData("roomid", chatid);
            sm.AddData("user", u);
            Parent.WriteMessage(sm);
        }

        /// <summary>
        /// Leave a chat room.
        /// </summary>
        /// <param name="rid"></param>
        public void LeaveChatRoom(long rid)
        {
            var sm = new SocketMessage("leavechat");
            sm.AddData("roomid", rid);
            Parent.WriteMessage(sm);
            Rooms.RemoveAll(r => r.ID == rid);
        }

        /// <summary>
        /// Send a chat message to a room.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="message"></param>
        public void SendChatMessage(long rid, string message)
        {
            var sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", rid);
            sm.AddData("mess", message);
            Parent.WriteMessage(sm);
        }

        /// <summary>
        /// Don't call this, Chatting calls this.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="u"></param>
        /// <param name="allusers"></param>
        public void UserJoinedChat(long rid, User u, List<User> allusers)
        {
            if (!Rooms.Exists(r => r.ID == rid))
            {
                Rooms.Add(new ChatRoom(rid));
            }
            ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
            cr.ResetUserList(allusers);
            if (u.Uid == Parent.Me.Uid)
            {
                if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.MeJoinedChat, u, null);
            }
            else
            {
                if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserJoinedChat, u, null);
            }
        }

        /// <summary>
        /// Don't call this, Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="u"></param>
        public void UserLeftChat(long rid, User u)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
            if (cr != null)
            {
                cr.RemoveUser(u);
                if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserLeftChat, u, null);
            }
        }

        /// <summary>
        /// Don't call this...Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="u"></param>
        /// <param name="message"></param>
        public void RecieveChatMessage(long rid, User u, string message)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
            if (cr != null)
            {
                if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.ChatMessage, u, message);
            }
        }

        public void UserStatusChange(long rid, User u, UserStatus ustatus)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
            if (cr != null)
            {
                cr.UserStatusChange(u, ustatus);
            }
        }
    }
}