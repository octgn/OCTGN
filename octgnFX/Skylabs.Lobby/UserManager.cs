namespace Skylabs.Lobby
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    using Octgn.Site.Api.Models;

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
            UserCache = new Dictionary<User, ApiUser>();
            RefreshApiTimer = new Timer(45000);
            RefreshApiTimer.Elapsed += RefreshApiTimerOnElapsed;
        }

        #endregion Singleton

        internal Timer RefreshApiTimer;

        private void RefreshApiTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            
        }

        internal ApiUser ApiUser(User user)
        {
            lock (UserCacheLocker)
            {
                if (UserCache.ContainsKey(user))
                {
                    return UserCache[user];
                }
                UserCache.Add(user,null);
                return null;
            }
        }

        internal Dictionary<User, ApiUser> UserCache { get; set; }

        internal readonly object UserCacheLocker = new object();

    }
}