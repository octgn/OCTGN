namespace Octgn.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;

    using NuGet;

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

        internal IEnumerable<IPackage> GetPackages()
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(MainFeed);
            var packages = repo.GetPackages().Where(x => x.IsAbsoluteLatestVersion);
            return packages.ToList();
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}