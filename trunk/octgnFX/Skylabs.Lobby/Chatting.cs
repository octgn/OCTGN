using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Net;

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
        public void CreateChatRoom(User otherUser)
        {
            SocketMessage sm = new SocketMessage("twopersonchat");
            sm.AddData("user", otherUser);
            Parent.WriteMessage(sm);
        }
        public void JoinChatRoom(long id)
        {
            SocketMessage sm = new SocketMessage("joinchatroom");
            sm.AddData("roomid", id);
            Parent.WriteMessage(sm);
        }
        public void AddUserToChat(User u, long chatid)
        {
            SocketMessage sm = new SocketMessage("addusertochat");
            sm.AddData("roomid", chatid);
            sm.AddData("user", u);
            Parent.WriteMessage(sm);
        }
        public void LeaveChatRoom(long rid)
        {
                SocketMessage sm = new SocketMessage("leavechat");
                sm.AddData("roomid",rid);
                Parent.WriteMessage(sm);
                Rooms.RemoveAll(r => r.ID == rid);

        }
        public void SendChatMessage(long rid,string message)
        {
                SocketMessage sm = new SocketMessage("chatmessage");
                sm.AddData("roomid", rid);
                sm.AddData("mess", message);
                Parent.WriteMessage(sm);
        }
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
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.MeJoinedChat, u,null);
                }
                else
                {
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserJoinedChat, u,null);
                }
        }
        public void UserLeftChat(long rid,User u)
        {
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    cr.RemoveUser(u);
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.UserLeftChat, u,null);
                }
        }
        public void RecieveChatMessage(long rid,User u,string message)
        {
                ChatRoom cr = Rooms.FirstOrDefault(r => r.ID == rid);
                if (cr != null)
                {
                    
                    if (eChatEvent != null) eChatEvent.Invoke(cr, ChatEvent.ChatMessage, u,message);
                }
        }
    }
}
