﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Octgn.Communication;
using Octgn.Site.Api.Models;

namespace Octgn.Online
{
    public class ApiUserCache
    {
        private static ILogger Log = LoggerFactory.Create(nameof(ApiUserCache));

        public static ApiUserCache Instance { get; set; } = new ApiUserCache();

        internal ApiUserCache()
        {
            UserCache = new Dictionary<User, ApiUser>();
            RefreshApiTimer = new Timer(10000);
            RefreshApiTimer.Elapsed += RefreshApiTimerOnElapsed;
            RefreshApiTimer.Start();
        }

        public event Action OnUpdate;

        internal Timer RefreshApiTimer;

        internal bool runningTimer = false;

        private void RefreshApiTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (runningTimer) return;
            runningTimer = true;
            if ((int)RefreshApiTimer.Interval == 10000) 
                RefreshApiTimer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            Log.Info("Refreshing User Manager");
            var unlist = new string[0];
            lock (UserCacheLocker) unlist = UserCache.Keys.Select(x => x.UserName).ToArray();

            var users = new Octgn.Site.Api.ApiClient().UsersFromUsername(unlist);
            if (users == null)
            {
                Log.Warn("User Manager Refresh failed");
                runningTimer = false;
                return;
            }

            lock (UserCacheLocker)
            {
                foreach (var u in UserCache.ToDictionary(x=>x.Key,x=>x.Value))
                {
                    var apiuser =
                        users.FirstOrDefault(
                            x => x.UserName.Equals(u.Key.UserName, StringComparison.InvariantCultureIgnoreCase));
                    UserCache[u.Key] = apiuser;
                }
            }
            runningTimer = false;
            this.FireOnUpdate();
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

        protected virtual void FireOnUpdate()
        {
            this.OnUpdate?.Invoke();
        }
    }
}