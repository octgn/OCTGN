using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Octgn.Communication;
using Octgn.Site.Api.Models;
using Octgn.Site.Api;

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

        private async void RefreshApiTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try {
                if (runningTimer) return;
                runningTimer = true;

                if ((int)RefreshApiTimer.Interval == 10000)
                    RefreshApiTimer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;

                Log.Info("Refreshing User Manager");
                var userIds = new string[0];
                lock (UserCacheLocker)
                    userIds = UserCache.Keys.Select(x => x.Id).ToArray();

                var client = new ApiClient();

                var users = await client.UsersFromUserIds(userIds);

                lock (UserCacheLocker) {
                    foreach (var u in UserCache.ToDictionary(x => x.Key, x => x.Value)) {
                        UserCache[u.Key] = users.FirstOrDefault(x => x.Id.Equals(u.Key.Id));
                    }
                }
                runningTimer = false;
                this.FireOnUpdate();
            } catch (Exception ex) {
                Log.Error($"{nameof(RefreshApiTimerOnElapsed)}: Unhandled Exception", ex);
            }
        }

        public ApiUser ApiUser(User user)
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