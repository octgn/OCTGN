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

        private readonly LobbyClient _parent;

        private long _lastRid = 0;
        public long NextRID
        {
            get { var r = _lastRid;
                _lastRid++;
                if (_lastRid == long.MaxValue)
                    _lastRid = 0;
                return r;
            }
        }
        public Chatting(LobbyClient c)
        {
            _parent = c;
            Rooms = new List<ChatRoom> {new ChatRoom(0)};
        }

        public List<ChatRoom> Rooms { get; private set; }
        public event ChatEventDelegate EChatEvent;

        public ChatRoom GetChatRoomFromRid(long rid)
        {
            lock (Rooms)
            {
                return Rooms.FirstOrDefault(r => r.Id == rid);
            }
        }

        /// <summary>
        ///   Create a chat room with another user.
        /// </summary>
        /// <param name="otherUser"> </param>
        public void CreateChatRoom(NewUser otherUser)
        {
            ChatRoom cr = new ChatRoom(_lastRid);
            _lastRid++;
            if (_lastRid == long.MaxValue)
                _lastRid = 0;
        }

        /// <summary>
        ///   Join a chat room based on an RID
        /// </summary>
        /// <param name="id"> </param>
        public void JoinChatRoom(long id)
        {
            var sm = new SocketMessage("joinchatroom");
            sm.AddData("roomid", id);
            _parent.WriteMessage(sm);
        }

        /// <summary>
        ///   Add a user to a chat room
        /// </summary>
        /// <param name="u"> </param>
        /// <param name="chatid"> </param>
        public void AddUserToChat(User u, long chatid)
        {
            var sm = new SocketMessage("addusertochat");
            sm.AddData("roomid", chatid);
            sm.AddData("user", u);
            _parent.WriteMessage(sm);
        }

        /// <summary>
        ///   Leave a chat room.
        /// </summary>
        /// <param name="rid"> </param>
        public void LeaveChatRoom(long rid)
        {
            var sm = new SocketMessage("leavechat");
            sm.AddData("roomid", rid);
            _parent.WriteMessage(sm);
            Rooms.RemoveAll(r => r.Id == rid);
        }

        /// <summary>
        ///   Send a chat message to a room.
        /// </summary>
        /// <param name="rid"> </param>
        /// <param name="message"> </param>
        public void SendChatMessage(long rid, string message)
        {
            if (message.Length >= 2000) return;
            var sm = new SocketMessage("chatmessage");
            sm.AddData("roomid", rid);
            sm.AddData("mess", message);
            _parent.WriteMessage(sm);
        }

        /// <summary>
        ///   Don't call this, Chatting calls this.
        /// </summary>
        /// <param name="rid"> </param>
        /// <param name="u"> </param>
        /// <param name="allusers"> </param>
        public void UserJoinedChat(long rid, User u, List<User> allusers)
        {
            if (!Rooms.Exists(r => r.Id == rid))
            {
                Rooms.Add(new ChatRoom(rid));
            }
            ChatRoom cr = Rooms.FirstOrDefault(r => r.Id == rid);
            if (cr == null) return;
            cr.ResetUserList(allusers);
            if (u.Uid == _parent.Me.Uid)
            {
                if (EChatEvent != null) EChatEvent.Invoke(cr, ChatEvent.MeJoinedChat, u, null);
            }
            else
            {
                if (EChatEvent != null) EChatEvent.Invoke(cr, ChatEvent.UserJoinedChat, u, null);
            }
        }

        /// <summary>
        ///   Don't call this, Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"> </param>
        /// <param name="u"> </param>
        public void UserLeftChat(long rid, User u)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.Id == rid);
            if (cr == null) return;
            cr.RemoveUser(u);
            if (EChatEvent != null) EChatEvent.Invoke(cr, ChatEvent.UserLeftChat, u, null);
        }

        /// <summary>
        ///   Don't call this...Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"> </param>
        /// <param name="u"> </param>
        /// <param name="message"> </param>
        public void RecieveChatMessage(long rid, User u, string message)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.Id == rid);
            if (cr == null) return;
            if (EChatEvent != null) EChatEvent.Invoke(cr, ChatEvent.ChatMessage, u, message);
        }

        public void UserStatusChange(long rid, User u, UserStatus ustatus)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.Id == rid);
            if (cr != null)
            {
                cr.UserStatusChange(u, ustatus);
            }
        }
    }
}