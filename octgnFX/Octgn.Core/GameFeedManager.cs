namespace Octgn.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Timers;

    using NuGet;

    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using Octgn.Library.ExtensionMethods;
    using Octgn.Library.Networking;

    using log4net;

    public interface IGameFeedManager : IDisposable
    {
        event Action<String> OnUpdateMessage;
        void CheckForUpdates(bool localOnly = false, Action<int,int> onProgressUpdate = null);
        IEnumerable<NamedUrl> GetFeeds(bool localOnly = false);
        void AddFeed(string name, string feed);
        void RemoveFeed(string name);
        bool ValidateFeedUrl(string url);
        IEnumerable<IPackage> GetPackages(NamedUrl url);
        void ExtractPackage(string directory, IPackage package, Action<int,int> onProgressUpdate = null);
        void AddToLocalFeed(string file);
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

        public event EventHandler OnUpdateFeedList;

        public event Action<string> OnUpdateMessage;

        protected virtual void FireOnUpdateMessage(string obj, params object[] args)
        {
            var handler = this.OnUpdateMessage;
            if (handler != null)
            {
                handler(string.Format(obj,args));
            }
        }

        public void CheckForUpdates(bool localOnly = false, Action<int,int> onProgressUpdate = null)
        {
            if (onProgressUpdate == null) onProgressUpdate = (i, i1) => { };
            Log.Info("Checking for updates");
            try
            {
                foreach (var g in DataManagers.GameManager.Get().Games)
                {
                    FireOnUpdateMessage("Checking for updates for game {0}", g.Name);
                    Log.DebugFormat("Checking for updates for game {0} {1}",g.Id,g.Name);
                    foreach (var f in this.GetFeeds(localOnly))
                    {
                        Log.DebugFormat("Getting feed {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                        var repo = PackageRepositoryFactory.Default.CreateRepository(f.Url);
                        Log.DebugFormat("Repo Created {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                        IPackage newestPackage = default(IPackage);
                        try
                        {
							X.Instance.Retry(
							    () =>
							    {
                                    newestPackage =
                                        repo.GetPackages()
                                            .Where(x => x.Id.ToLower() == g.Id.ToString().ToLower())
                                            .ToList()
                                            .OrderByDescending(x => x.Version.Version)
                                            .FirstOrDefault(x => x.IsAbsoluteLatestVersion);
							    });
                        }
                        catch (WebException e)
                        {
                            Log.WarnFormat("Could not get feed {0} {1}",f.Name,f.Url);
                            Log.Warn("",e);
                            continue;
                        }
                        Log.DebugFormat("Grabbed newest package for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                        if (newestPackage == null)
                        {
                            Log.DebugFormat("No package found for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                            continue;
                        }
                        Log.DebugFormat("Got feed {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                        Log.DebugFormat("Installed Version: {0} Feed Version: {1} for {2} {3} {4} {5}", g.Version, newestPackage.Version.Version, g.Id, g.Name, f.Name, f.Url);
                        var gameVersion = new SemanticVersion(g.Version);
                        if (newestPackage.Version.Version.CompareTo(gameVersion.Version) > 0)
                        {
                            FireOnUpdateMessage(
                                "Updating {0} from {1} to {2}", g.Name, g.Version, newestPackage.Version.Version);
                            Log.DebugFormat(
                                "Update found. Updating from {0} to {1} for {2} {3} {4} {5}", g.Version, newestPackage.Version.Version,g.Id, g.Name, f.Name, f.Url);
                            DataManagers.GameManager.Get().InstallGame(newestPackage,onProgressUpdate);
                            Log.DebugFormat("Updated game finished for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url);
                            break;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Log.Warn("Error checking for updates", e);
            }
            finally
            {
                Log.Info("Check for updates finished");
            }
        }

        /// <summary>
        /// Gets all saved game feeds
        /// </summary>
        /// <returns>Saved game feeds</returns>
        public IEnumerable<NamedUrl> GetFeeds(bool localOnly = false)
        {
            Log.Info("Getting Feeds");
            if (localOnly) return FeedProvider.Instance.LocalFeeds;
            else return FeedProvider.Instance.Feeds;
        }

        /// <summary>
        /// Add a feed url to the system.
        /// </summary>
        /// <exception cref="UserMessageException">If the feed name already exists or the feed is invalid.</exception>
        /// <param name="name">Feed name</param>
        /// <param name="feed">Feed url</param>
        public void AddFeed(string name, string feed)
        {
            try
            {
                Log.InfoFormat("Validating feed for {0} {1}", name, feed);
                if (!SingletonContext.ValidateFeedUrl(feed))
                {
                    Log.InfoFormat("Feed not valid for {0} {1}", name, feed);
                    throw new UserMessageException("{0} is not a valid feed.", feed);
                }
                Log.InfoFormat("Checking if feed name already exists for {0} {1}", name, feed);
                if (FeedProvider.Instance.Feeds.Any(x => x.Name.ToLower() == name.ToLower()))
                {
                    Log.InfoFormat("Feed name already exists for {0} {1}", name, feed);
                    throw new UserMessageException("Feed name {0} already exists.", name);
                }
                Log.InfoFormat("Adding feed {0} {1}", name, feed);
                FeedProvider.Instance.AddFeed(new NamedUrl(name, feed));
                Log.InfoFormat("Firing update feed list {0} {1}", name, feed);
                this.FireOnUpdateFeedList();
                Log.InfoFormat("Feed {0} {1} added.", name, feed);

            }
            finally
            {
                Log.InfoFormat("Finished {0} {1}",name,feed);
            }
        }

        /// <summary>
        /// Remove a feed url from the system.
        /// </summary>
        /// <param name="name">Feed name</param>
        public void RemoveFeed(string name)
        {
            Log.InfoFormat("Removing feed {0}", name);
            FeedProvider.Instance.RemoveFeed(new NamedUrl(name, ""));
            Log.InfoFormat("Firing update feed list {0}",name);
            this.FireOnUpdateFeedList();
            Log.InfoFormat("Removed feed {0}", name);
        }

        public void AddToLocalFeed(string file)
        {
            try
            {
                Log.InfoFormat("Verifying {0}",file);
                this.VerifyPackage(file);
                Log.InfoFormat("Creating Install Path {0}", file);
                var fi = new FileInfo(file);
                var newFileName = fi.Name.Replace(fi.Extension, ".nupkg");
                var newpath = Path.Combine(Paths.Get().LocalFeedPath, newFileName);
                Log.InfoFormat("Adding to local feed {0} to {1}", file, newpath);
                if (!File.Exists(file))
                {
                    Log.InfoFormat("o8g magically disappeared {0}", file);
                    return;
                }
                fi.MegaCopyTo(newpath);
                Log.InfoFormat("Firing update feed list {0}", file);
                this.FireOnUpdateFeedList();
                Log.InfoFormat("Feed {0} Added at {1}", file, newpath);

            }
            finally
            {
                Log.InfoFormat("Finished {0}",file);
            }
        }

        internal void VerifyPackage(string filename)
        {
            try
            {
                Log.InfoFormat("Creating verify path {0}", filename);
                var fi = new FileInfo(filename);
                var tempPath =
                    new FileInfo(
                        Path.Combine(
                            Path.GetTempPath(),
                            "octgn",
                            Guid.NewGuid().ToString(),
                            fi.Name.Replace(fi.Extension, ".nupkg")));
                if (!File.Exists(filename))
                {
                    Log.InfoFormat("Package magically disappeared {0}", filename);
                    return;
                }
                Log.InfoFormat("Creating directory {0}", filename);
                if (!Directory.Exists(tempPath.Directory.FullName)) Directory.CreateDirectory(tempPath.Directory.FullName);
                Log.InfoFormat("Copying file {0}", filename);
                fi.MegaCopyTo(tempPath);
                Log.InfoFormat("Creating repo to make sure it loads {0}", filename);
                var repo = new LocalPackageRepository(tempPath.Directory.FullName);
                Log.InfoFormat("Loading repo into array to make sure it works {0}", filename);
                var arr = repo.GetPackages().ToArray();
                Log.InfoFormat("Fully verified {0}", filename);
            }
            catch (Exception e)
            {
                Log.WarnFormat("Package not valid {0}", filename);
                throw new UserMessageException("The file {0} is invalid.", filename);
            }
            finally
            {
                Log.InfoFormat("Finished {0}", filename);
            }
        }

        public IEnumerable<IPackage> GetPackages(NamedUrl url)
        {
            try
            {
                if (url == null)
                {
                    Log.Info("Getting packages for null NamedUrl");
                    return new List<IPackage>();
                }
                Log.InfoFormat("Getting packages for feed {0}:{1}", url.Name, url.Url);
                var ret = new List<IPackage>();
                ret = PackageRepositoryFactory.Default.CreateRepository(url.Url).GetPackages().ToList();
                Log.InfoFormat("Finished getting packages for feed {0}:{1}", url.Name, url.Url);
                return ret;

            }
            finally
            {
                Log.InfoFormat("Finished");
            }
        }

        public void ExtractPackage(string directory, IPackage package, Action<int,int> onProgressUpdate = null)
        {
            try
            {
                if (onProgressUpdate == null) onProgressUpdate = (i, i1) => { };
                Log.InfoFormat("Extracting package {0} {1}", package.Id,directory);
                onProgressUpdate(-1, 1);
                var files = package.GetFiles().ToArray();
                var curFileNum = 0;
				onProgressUpdate(curFileNum, files.Length);
                foreach (var file in files)
                {
                    Log.InfoFormat("Got file {0} {1} {2}",file.Path, package.Id, directory);
                    var p = Path.Combine(directory, file.Path);
                    var fi = new FileInfo(p);
                    var dir = fi.Directory.FullName;
                    Log.InfoFormat("Creating directory {0} {1} {2}",dir, package.Id, directory);
                    Directory.CreateDirectory(dir);
                    var byteList = new List<byte>();
                    Log.InfoFormat("Reading file {0} {1}", package.Id, directory);
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
                        Log.InfoFormat("Writing file {0} {1}", package.Id, directory);
                        File.WriteAllBytes(p, byteList.ToArray());
                        Log.InfoFormat("Wrote file {0} {1}", package.Id, directory);
                    }
                    curFileNum++;
                    onProgressUpdate(curFileNum, files.Length);
                }
                Log.InfoFormat("No Errors {0} {1}", package.Id, directory);
            }
            finally
            {
                onProgressUpdate(-1, 1);
                Log.InfoFormat("Finished {0} {1}", package.Id, directory);
            }
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
            Log.InfoFormat("Validating feed url {0}", feed);
            if (PathValidator.IsValidUrl(feed) && PathValidator.IsValidSource(feed))
            {
                Log.InfoFormat("Path Validator says feed {0} is valid", feed);
                try
                {
                    Log.InfoFormat("Trying to query feed {0}", feed);
                    var repo = PackageRepositoryFactory.Default.CreateRepository(feed);
                    Log.InfoFormat("Loading feed to list {0}", feed);
                    var list = repo.GetPackages().ToList();
                    // This happens so that enumerating the list isn't optimized away.
                    foreach (var l in list)
                        System.Diagnostics.Trace.WriteLine(l.Id);
                    Log.InfoFormat("Queried feed {0}, feed is valid", feed);
                    return true;
                }
                catch (Exception e)
                {
                    Log.WarnFormat("{0} is an invalid feed.", feed);
                }
            }
            Log.InfoFormat("Path validator failed for feed {0}", feed);
            return false;
        }

        internal void FireOnUpdateFeedList()
        {
            Log.Info("Enter");
            if (OnUpdateFeedList != null)
            {
                OnUpdateFeedList(this, null);
            }
            Log.Info("Exit");
        }

        public void Dispose()
        {
            Log.Info("Dispose called");
            OnUpdateFeedList = null;
            Log.Info("Dispose finished");
        }
    }
}
