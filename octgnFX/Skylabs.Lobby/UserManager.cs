namespace Skylabs.Lobby
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using agsXMPP;

    public class UserManager
    {
        #region Singleton

        internal static UserManager SingletonContext { get; set; }

        private static readonly object UserManagerSingletonLocker = new object();

        public static UserManager Get()
        {
            lock (UserManagerSingletonLocker) return SingletonContext ?? (SingletonContext = new UserManager());
        }

        internal UserManager()
        {
            UserCache = new Dictionary<User,bool>();
        }

        #endregion Singleton

        internal bool IsUserSubbed(User user)
        {
            lock (UserCacheLocker)
            {
                if (UserCache.ContainsKey(user))
                {
                    return UserCache[user];
                }
                UserCache.Add(user,false);
                return false;
            }
        }

        internal void SetUserSubbed(User user, bool subbed)
        {
            lock (UserCacheLocker)
            {
                if (UserCache.ContainsKey(user))
                {
                    UserCache[user] = subbed;
                    return;
                }
                UserCache.Add(user,subbed);
            }
        }

        internal User FromUser(User user)
        {
            lock (UserCacheLocker)
            {
                if (!UserCache.ContainsKey(user))
                    UserCache.Add(user,false);
                return user;
            }
        }

        internal Dictionary<User, bool> UserCache { get; set; }

        internal readonly object UserCacheLocker = new object();

    }
}