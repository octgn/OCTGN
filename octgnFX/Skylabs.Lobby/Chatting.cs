using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Net;
using System.Diagnostics;

namespace Skylabs.Lobby
{
    public class Chatting
    {
        public enum ChatEvent{UserJoinedChat,UserLeftChat,MeJoinedChat, ChatMessage};
        public List<ChatRoom> Rooms { get; private set; }
        private LobbyClient Parent;
        public delegate void ChatEventDelegate(ChatRoom cr, ChatEvent e, User u, object data);
        public event ChatEventDelegate eChatEvent;
        public Chatting(LobbyClient c)
        {
            Parent = c;
            Rooms = new List<ChatRoom>();
            Rooms.Add(new ChatRoom(0));
        }
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
            SocketMessage sm = new SocketMessage("twopersonchat");
            sm.AddData("user", otherUser);
            Parent.WriteMessage(sm);
        }
        /// <summary>
        /// Join a chat room based on an RID
        /// </summary>
        /// <param name="id"></param>
        public void JoinChatRoom(long id)
        {
            Debug.WriteLine("Joining chat room {0}", id);
            SocketMessage sm = new SocketMessage("joinchatroom");
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
            SocketMessage sm = new SocketMessage("addusertochat");
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
                SocketMessage sm = new SocketMessage("leavechat");
                sm.AddData("roomid",rid);
                Parent.WriteMessage(sm);
                Rooms.RemoveAll(r => r.ID == rid);

        }
        /// <summary>
        /// Send a chat message to a room.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="message"></param>
        public void SendChatMessage(long rid,string message)
        {
                SocketMessage sm = new SocketMessage("chatmessage");
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
        public void UserJoinedChat(long rid,User u,List<User> allusers)
        {
                if (!Rooms.Exists(r => r.ID == rid))
                {
                    Rooms.Add(new ChatRoom(rid));
                    System.Diagnostics.Debug.Write("new chat with rid=" + rid.ToString());
                }
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                cr.ResetUserList(allusers);
                if (u.Uid == Parent.Me.Uid)
                {
                    System.Diagnostics.Debug.WriteLine("Connected to room.");
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.MeJoinedChat, u,null);
                }
                else
                {
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserJoinedChat, u,null);
                }
        }
        /// <summary>
        /// Don't call this, Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="u"></param>
        public void UserLeftChat(long rid,User u)
        {
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    cr.RemoveUser(u);
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserLeftChat, u,null);
                }
        }
        /// <summary>
        /// Don't call this...Chatting.cs calls this.
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="u"></param>
        /// <param name="message"></param>
        public void RecieveChatMessage(long rid,User u,string message)
        {
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.ChatMessage, u,message);
                }
        }
        public void UserStatusChange(long rid, User u, UserStatus ustatus)
        {
            ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
            if(cr != null)
            {
                cr.UserStatusChange(u, ustatus);
                
            }
        }
    }
}
