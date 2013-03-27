namespace Octgn.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Timers;

    using NuGet;

    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using Octgn.Library.Networking;

    using log4net;

    public interface IGameFeedManager : IDisposable
    {
        bool IsRunning { get; }
        void Start();
        void Stop();
        void CheckForUpdates();
        IEnumerable<NamedUrl> GetFeeds();
        void AddFeed(string name, string feed);
        void RemoveFeed(string name);
        bool ValidateFeedUrl(string url);
        IEnumerable<IPackage> GetPackages(NamedUrl url);
        void ExtractPackage(string directory, IPackage package);
        event EventHandler OnUpdateFeedList;
    }

    public class GameFeedManager : IGameFeedManager
    {
        #region Singleton

        internal static IGameFeedManager SingletonContext { get; set; }

        private static readonly object GameFeedManagerSingletonLocker = new object();

        public static IGameFeedManager Get()
        {
            lock (GameFeedManagerSingletonLocker)
            {
                if (SingletonContext != null) return SingletonContext;
            }
            return new GameFeedManager();
        }
        public GameFeedManager()
        {
            lock (GameFeedManagerSingletonLocker)
            {
                if (SingletonContext != null)
                    throw new InvalidOperationException("Game feed manager already exists!");
                SingletonContext = this;
            }
        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal int RefreshTime = 10 * 60 * 1000; //Believe that's 10 minutes

        public bool IsRunning { get; internal set; }
        public event EventHandler OnUpdateFeedList;
        internal Timer RefreshTimer { get; set; }
        
        #region StartStop
        public void Start()
        {
            Log.Info("Try Start");
            if (IsRunning) return;
            Log.Info("Start");
            IsRunning = true;
            ConstructTimer();
            Log.Info("Start Finished");
        }

        public void Stop()
        {
            Log.Info("Try Stop");
            if (!IsRunning) return;
            Log.Info("Stop");
            IsRunning = false;
            this.DestroyTimer();
            Log.Info("Stop Finished");
        }
        #endregion StartStop

        #region Timer
        internal virtual void ConstructTimer()
        {
            Log.Info("Constructing Timer");
            this.DestroyTimer();
            RefreshTimer = new Timer(RefreshTime);
            RefreshTimer.Elapsed += RefreshTimerOnElapsed;
            RefreshTimer.Start();
            this.RefreshTimerOnElapsed(null, null);
            Log.Info("Constructing Timer Complete");
        }

        internal virtual void DestroyTimer()
        {
            Log.Info("Destroying Timer");
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
            Log.Info("Timer destruction complete");
        }
        internal virtual void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Info("Timer Ticks");
        }
        #endregion Timer

        public void CheckForUpdates()
        {
            foreach (var package in this.GetPackages())
            {
                //package.
            }
        }

        /// <summary>
        /// Gets all saved game feeds
        /// </summary>
        /// <returns>Saved game feeds</returns>
        public IEnumerable<NamedUrl> GetFeeds()
        {
            Log.Info("Getting Feeds");
            return SimpleConfig.Get().GetFeeds();
        }

        /// <summary>
        /// Add a feed url to the system.
        /// </summary>
        /// <exception cref="UserMessageException">If the feed name already exists or the feed is invalid.</exception>
        /// <param name="name">Feed name</param>
        /// <param name="feed">Feed url</param>
        public void AddFeed(string name, string feed)
        {
            Log.InfoFormat("Adding Feed {0} {1}",name,feed);
            if (!SingletonContext.ValidateFeedUrl(feed))
                throw new UserMessageException("{0} is not a valid feed.",feed);
            if (SimpleConfig.Get().GetFeeds().Any(x => x.Name.ToLower() == name.ToLower()))
                throw new UserMessageException("Feed name {0} already exists.",name);
            SimpleConfig.Get().AddFeed(new NamedUrl(name, feed));
            this.FireOnUpdateFeedList();
            Log.InfoFormat("Feed {0} {1} added.",name,feed);
        }

        /// <summary>
        /// Remove a feed url from the system.
        /// </summary>
        /// <param name="name">Feed name</param>
        public void RemoveFeed(string name)
        {
            Log.InfoFormat("Removing feed {0}",name);
            SimpleConfig.Get().RemoveFeed(new NamedUrl(name, ""));
            this.FireOnUpdateFeedList();
            Log.InfoFormat("Removed feed {0}",name);
        }

        public IEnumerable<IPackage> GetPackages(NamedUrl url)
        {
            if (url == null)
            {
                Log.Info("Getting packages for null NamedUrl");
                return new List<IPackage>();
            }
            Log.InfoFormat("Getting packages for feed {0}:{1}",url.Name,url.Url);
            var ret = new List<IPackage>();
            ret = PackageRepositoryFactory.Default.CreateRepository(url.Url).GetPackages().ToList();
            Log.InfoFormat("Finished getting packages for feed {0}:{1}", url.Name, url.Url);
            return ret;
        }

        public void ExtractPackage(string directory, IPackage package)
        {
            foreach (var file in package.GetFiles())
            {
                var p = Path.Combine(directory, file.Path);
                var fi = new FileInfo(p);
                var dir = fi.Directory.FullName;
                Directory.CreateDirectory(dir);
                var byteList = new List<byte>();
                using (var sr = new BinaryReader(file.GetStream()))
                {
                    var buffer = new byte[1024];
                    var len = sr.Read(buffer, 0, 1024);
                    while (len > 0)
                    {
                        byteList.AddRange(buffer.Take(len));
                        Array.Clear(buffer, 0, buffer.Length);
                        len = sr.Read(buffer, 0, 1024);
                    }
                    File.WriteAllBytes(p,byteList.ToArray());
                }
            }
        }

        /// <summary>
        /// Get all packages from all feeds.
        /// </summary>
        /// <returns>All packages from all feeds.</returns>
        internal virtual IQueryable<IPackage> GetPackages()
        {
            // TODO - [GAME FEED] - This should be made for each feed, not all combined - Kelly Elton - 3/24/2013   
            //var repo = PackageRepositoryFactory.Default.CreateRepository(MainFeed);
            //var packages = repo.GetPackages().Where(x => x.IsAbsoluteLatestVersion);
            //return packages;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Make sure a feed url is valid.
        /// This doesn't check to make sure it has octgn games on it, it only
        /// checks to make sure it's a valid nuget feed, and sometimes it's even 
        /// wrong when it check that, so don't 100% rely on this for validation.
        /// </summary>
        /// <param name="feed">Feed url</param>
        /// <returns>Returns true if it is, or false if it isn't</returns>
        public bool ValidateFeedUrl(string feed)
        {
            Log.InfoFormat("Validating feed url {0}",feed);
            if (PathValidator.IsValidUrl(feed) && PathValidator.IsValidSource(feed))
            {
                Log.InfoFormat("Path Validator says feed {0} is valid",feed);
                try
                {
                    Log.InfoFormat("Trying to query feed {0}",feed);
                    var repo = PackageRepositoryFactory.Default.CreateRepository(feed);
                    var list = repo.GetPackages().ToList();
                    // This happens so that enumerating the list isn't optimized away.
                    foreach(var l in list)
                        System.Diagnostics.Trace.WriteLine(l.Id);
                    Log.InfoFormat("Queried feed {0}, feed is valid",feed);
                    return true;
                }
                catch(Exception e)
                {
                    Log.WarnFormat("{0} is an invalid feed.",feed);
                }
            }
            Log.InfoFormat("Path validator failed for feed {0}",feed);
            return false;
        }

        internal void FireOnUpdateFeedList()
        {
            if (OnUpdateFeedList != null)
            {
                OnUpdateFeedList(this, null);
            }
        }

        public void Dispose()
        {
            Log.Info("Dispose called");
            OnUpdateFeedList = null;
            SingletonContext.Stop();
            Log.Info("Dispose finished");
        }
    }
}