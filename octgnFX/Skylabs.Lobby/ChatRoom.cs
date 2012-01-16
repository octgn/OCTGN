using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.Lobby
{
    public class ChatRoom
    {
        public long ID { get; private set; }
        private List<User> Users {get;set;}

        public int UserCount
        {
            get
            {
                lock (userLocker)
                    return Users.Count;
            }
        }

        private object userLocker = new object();

        public bool ContainsUser(User u)
        {
            lock (userLocker)
            {
                return Users.Contains(u);
            }
        }

        public ChatRoom(long id)
        {
            ID = id;
            Users = new List<User>();
        }
        public User[] GetUserList()
        {
            lock(userLocker)
                return Users.ToArray();
        }
        public void UserStatusChange(User userToChange, UserStatus newUserData)
        {
            lock (userLocker)
            {
                if (Users.Contains(userToChange))
                {
                    User utochange = Users.FirstOrDefault(us => us.Uid == userToChange.Uid);
                    if (utochange != null)
                    {
                        utochange.Status = newUserData;
                        utochange.CustomStatus = userToChange.CustomStatus;
                        utochange.DisplayName = userToChange.DisplayName;
                    }

                }
            }
        }
        public void ResetUserList(List<User> users)
        {
            lock (userLocker)
            {
                Users = users;
            }
        }
        public void RemoveUser(User user)
        {
            lock (userLocker)
                Users.Remove(user);
        }
    }
}
