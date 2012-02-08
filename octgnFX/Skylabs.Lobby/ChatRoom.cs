using System.Collections.Generic;
using System.Linq;

namespace Skylabs.Lobby
{
    public class ChatRoom
    {
        private readonly object _userLocker = new object();

        public ChatRoom(long id)
        {
            Id = id;
            Users = new List<User>();
        }

        public long Id { get; private set; }
        private List<User> Users { get; set; }

        public int UserCount
        {
            get
            {
                lock (_userLocker)
                    return Users.Count;
            }
        }

        public bool ContainsUser(User u)
        {
            lock (_userLocker)
            {
                return Users.Contains(u);
            }
        }

        public User[] GetUserList()
        {
            lock (_userLocker)
                return Users.ToArray();
        }

        public void UserStatusChange(User userToChange, UserStatus newUserData)
        {
            lock (_userLocker)
            {
                if (!Users.Contains(userToChange)) return;
                User utochange = Users.FirstOrDefault(us => us.Uid == userToChange.Uid);
                if (utochange == null) return;
                utochange.Status = newUserData;
                utochange.CustomStatus = userToChange.CustomStatus;
                utochange.DisplayName = userToChange.DisplayName;
            }
        }

        public void ResetUserList(List<User> users)
        {
            lock (_userLocker)
            {
                Users = users;
            }
        }

        public void RemoveUser(User user)
        {
            lock (_userLocker)
                Users.Remove(user);
        }
    }
}