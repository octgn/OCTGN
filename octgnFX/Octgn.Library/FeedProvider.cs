using Octgn.Library.Exceptions;

namespace Octgn.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using log4net;

    using Octgn.Library.Networking;

    public interface IFeedProvider
    {
        IEnumerable<NamedUrl> Feeds { get; }
        IEnumerable<NamedUrl> LocalFeeds { get; }

        void AddFeed(NamedUrl feed);

        void RemoveFeed(NamedUrl feed);
    }

    public class FeedProvider : IFeedProvider
    {
        #region Singleton

        internal static IFeedProvider SingletonContext { get; set; }

        private static readonly object FeedProviderSingletonLocker = new object();

        public static IFeedProvider Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (FeedProviderSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new FeedProvider();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DateTime nextCheck;

        private List<NamedUrl> feeds;

        public IEnumerable<NamedUrl> Feeds
        {
            get
            {
                return GetFeeds();
            }
        }

        public IEnumerable<NamedUrl> LocalFeeds
        {
            get
            {
                return GetFeeds(true);
            }
        }

        internal FeedProvider()
        {
            nextCheck = DateTime.MinValue;
            feeds = new List<NamedUrl>();
        }

        private IEnumerable<NamedUrl> GetFeeds(bool localOnly = false)
        {
            if (DateTime.Now > nextCheck)
            {
                var ret = new List<NamedUrl>();
                ret.Add(new NamedUrl("Local", Paths.Get().LocalFeedPath));
                if (!localOnly)
                {
                    ret.Add(new NamedUrl("OCTGN Official", Paths.Get().MainOctgnFeed));
                    ret.AddRange(this.GetFeedsList().ToList());
                }
                feeds = ret;
                nextCheck = DateTime.Now.AddMinutes(1);
            }
            if (!localOnly) return feeds.ToArray();
            return this.feeds.Where(x => x.Name == "Local").ToArray();
        }

        public void AddFeed(NamedUrl feed)
        {
            lock (this)
            {
                if (feed.Name.Equals("Local", StringComparison.InvariantCultureIgnoreCase) || feed.Name.Equals("OCTGN Official", StringComparison.InvariantCultureIgnoreCase))
                    throw new UserMessageException("You can not replace built in feeds");
                var remList = feeds.Where(x => x.Name.Equals(feed.Name)).ToArray();
                foreach (var r in remList)
                {
                    feeds.Remove(r);
                }
                feeds.Add(feed);
				this.WriteFeedList();
            }
        }

        public void RemoveFeed(NamedUrl feed)
        {
            lock (this)
            {
                if(feed.Name.Equals("Local",StringComparison.InvariantCultureIgnoreCase) || feed.Name.Equals("OCTGN Official",StringComparison.InvariantCultureIgnoreCase))
                    throw new UserMessageException("Can not remove built in feeds.");
                var remList = feeds.Where(x => x.Name.Equals(feed.Name)).ToArray();
                foreach (var r in remList)
                {
                    feeds.Remove(r);
                }
                this.WriteFeedList();
            }
        }

        public List<NamedUrl> RemoveLocalFeeds(List<NamedUrl> feedList)
        {
            feedList = (from line in feedList
                     where (line.Name.ToLowerInvariant() != "octgn official" && line.Name.ToLowerInvariant() != "local")
                     select line).ToList();
            return feedList;
        }

        internal IEnumerable<NamedUrl> GetFeedsList()
        {
            Stream stream = null;
            var wasLocked = false;
            while (!X.Instance.File.OpenFile(Paths.Get().FeedListPath, FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                wasLocked = true;
                Log.Info("Getting feed list file still locked.");
                Thread.Sleep(2000);
            }
            if (wasLocked) Log.Debug("Getting feed list file unlocked.");
            using (var sr = new StreamReader(stream))
            {
                var lines = sr.ReadToEnd()
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !String.IsNullOrWhiteSpace(x.Trim()))
                    .Select(x => x.Split(new[] { (char)1 }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Length != 2 ? null : new NamedUrl(x[0].Trim(), x[1].Trim()))
                    .Where(x => x != null).ToList();

                lines.ForEach(line => line.Url = CorrectMyGetFeed(line.Url));

                lines.ForEach(CorrectFeedReplacements);

                // Don't read local and octgn official from file
                lines = this.RemoveLocalFeeds(lines);

                return lines;
            }
        }

        internal void WriteFeedList()
        {
            var lines = Feeds.ToList();

            // correct myGet URLS -- correct them both here before the check to make sure we don't get an http and https version of the same.
            lines.ForEach(line => line.Url = CorrectMyGetFeed(line.Url));

            // Don't write local and octgn official to file
            lines = this.RemoveLocalFeeds(lines);

            Stream stream = null;
            while (!X.Instance.File.OpenFile(Paths.Get().FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            using (var sr = new StreamWriter(stream))
            {
                lines.ForEach(line => sr.WriteLine(line.Name + (char)1 + line.Url));
            }
        }

        private void CorrectFeedReplacements(NamedUrl url)
        {
            if (url.Name.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
            {
                url.Url = Paths.Get().LocalFeedPath;
            }
            else if (url.Name.Equals("OCTGN Official", StringComparison.InvariantCultureIgnoreCase))
            {
                url.Url = Paths.Get().MainOctgnFeed;
            }
        }

        private string CorrectMyGetFeed(string url)
        {
            var bad = @"http://www.myget.org";
            var good = @"https://www.myget.org";

            if (url.ToLower().StartsWith(bad))
            {
                var remainder = url.Substring(bad.Length);
                url = good + remainder;
            }

            return url;
        }
    }
}