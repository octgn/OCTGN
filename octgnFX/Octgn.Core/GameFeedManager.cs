namespace Octgn.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;

    using NuGet;

    using Octgn.Library;
    using Octgn.Library.Exceptions;

    using log4net;

    public class GameFeedManager : IDisposable
    {
        #region Singleton

        internal static GameFeedManager SingletonContext { get; set; }

        private static readonly object GameFeedManagerSingletonLocker = new object();

        public static GameFeedManager Get()
        {
            lock (GameFeedManagerSingletonLocker) return SingletonContext ?? (SingletonContext = new GameFeedManager());
        }
        internal GameFeedManager()
        {
            
        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal string MainFeed = "http://www.myget.org/F/octgngames/";
        internal int RefreshTime = 10 * 60 * 1000; //Believe that's 10 minutes

        internal bool IsRunning { get; set; }
        internal Timer RefreshTimer { get; set; }

        #region StartStop
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            this.ConstructTimer();
            this.RefreshTimerOnElapsed(null,null);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            this.DestroyTimer();
        }
        #endregion StartStop

        #region Timer
        internal void ConstructTimer()
        {
            this.DestroyTimer();
            RefreshTimer = new Timer(RefreshTime);
            RefreshTimer.Elapsed += RefreshTimerOnElapsed;
            RefreshTimer.Start();
        }

        internal void DestroyTimer()
        {
            if (RefreshTimer != null)
            {
                try { RefreshTimer.Elapsed -= this.RefreshTimerOnElapsed; }
                catch { }
                try { RefreshTimer.Stop(); }
                catch { }
                try { RefreshTimer.Dispose(); }
                catch { }
                RefreshTimer = null;
            }
        }
        internal void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {

        }
        #endregion Timer

        public void CheckForUpdates()
        {
            foreach (var package in this.GetPackages())
            {
                //package.
            }
        }

        public void AddFeed(string feed)
        {
            if(!this.ValidateFeedUrl(feed))
                throw new UserMessageException("{0} is not a valid feed.",feed);
            SimpleConfig.AddFeed(feed);
        }

        public void RemoveFeed(string feed)
        {
            SimpleConfig.RemoveFeed(feed);
        }

        internal IQueryable<IPackage> GetPackages()
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(MainFeed);
            var packages = repo.GetPackages().Where(x => x.IsAbsoluteLatestVersion);
            return packages;
        }

        internal bool ValidateFeedUrl(string feed)
        {
            if (PathValidator.IsValidUrl(feed) && PathValidator.IsValidSource(feed))
            {
                try
                {
                    var repo = PackageRepositoryFactory.Default.CreateRepository(feed);
                    var list = repo.GetPackages().ToList();
                    foreach(var l in list)
                        System.Diagnostics.Trace.WriteLine(l.Id);
                    return true;
                }
                catch(Exception e)
                {
                    Log.WarnFormat("{0} is an invalid feed.",feed);
                }
            }
            return false;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}