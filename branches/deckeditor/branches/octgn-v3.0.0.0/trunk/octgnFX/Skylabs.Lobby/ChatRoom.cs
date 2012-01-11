using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.Lobby
{
    public class ChatRoom
    {
        public long ID { get; private set; }
        public List<User> Users {get;private set;}

        public ChatRoom(long id)
        {
            ID = id;
            Users = new List<User>();
        }
        public void ResetUserList(List<User> users)
        {
            lock(Users)
            {
                Users = users;
            }
        }
        public void RemoveUser(User user)
        {
            lock(Users)
                Users.Remove(user);
        }
    }
}
