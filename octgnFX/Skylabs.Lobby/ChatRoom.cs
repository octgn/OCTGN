using System.Collections.Generic;
using System.Linq;

namespace Skylabs.Lobby
{
    public class ChatRoom
    {
        private readonly object userLocker = new object();

        public ChatRoom(long id)
        {
            ID = id;
            Users = new List<User>();
        }

        public long ID { get; private set; }
        private List<User> Users { get; set; }

        public int UserCount
        {
            get
            {
                lock (userLocker)
                    return Users.Count;
            }
        }

        public bool ContainsUser(User u)
        {
            lock (userLocker)
            {
                return Users.Contains(u);
            }
        }

        public User[] GetUserList()
        {
            lock (userLocker)
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