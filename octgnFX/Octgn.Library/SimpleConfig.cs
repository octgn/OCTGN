namespace Octgn.Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Octgn.Library.Exceptions;
    using Octgn.Library.Networking;

    using Polenter.Serialization;

    public interface ISimpleConfig
    {
        string DataDirectory { get; set; }
        string GetConfigPath();

        string ReadValue(string valName, string def);

        void WriteValue(string valName, string value);

        IEnumerable<NamedUrl> GetFeeds();

        void AddFeed(NamedUrl feed);

        void RemoveFeed(NamedUrl feed);

        bool OpenFile(string path, FileMode fileMode, FileShare share, TimeSpan timeout, out Stream stream);
    }

    public class SimpleConfig : ISimpleConfig
    {
        #region Singleton

        internal static ISimpleConfig SingletonContext { get; set; }

        private static readonly object SimpleConfigSingletonLocker = new object();

        public static ISimpleConfig Get()
        {
            lock (SimpleConfigSingletonLocker) return SingletonContext ?? (SingletonContext = new SimpleConfig());
        }

        internal SimpleConfig()
        {
        }

        #endregion Singleton

        internal object LockObject = new Object();

        /// <summary>
        /// Special case since it's required in Octgn.Data, and Prefs can't go there
        /// </summary>
        public string DataDirectory
        {
            get
            {
                return ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));
            }
            set
            {
                WriteValue("datadirectory", value);
            }
        }

        public string GetConfigPath()
        {
            string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
            const string f = "settings.xml";
            string fullPath = Path.Combine(p, f);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            return fullPath;
        }

        /// <summary>
        ///   Reads a string value from the Octgn registry
        /// </summary>
        /// <param name="valName"> The name of the value </param>
        /// <returns> A string value </returns>
        public string ReadValue(string valName, string def)
        {
            lock (this.LockObject)
            {
                var ret = def;
                Stream f = null;
                try
                {
                    if (File.Exists(GetConfigPath()))
                    {
                        var serializer = new SharpSerializer();
                        
                        Hashtable config = new Hashtable();
                        if (OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
                        {
                            config = (Hashtable)serializer.Deserialize(f);
                        }
                        if (config.ContainsKey(valName))
                        {
                            return config[valName].ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("[SimpleConfig]ReadValue Error: " + e.Message);
                    try
                    {
                        File.Delete(GetConfigPath());
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("[SimpleConfig]ReadValue Error: Couldn't delete the corrupt config file.");
                    }
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        ///   Writes a string value to the Octgn registry
        /// </summary>
        /// <param name="valName"> Name of the value </param>
        /// <param name="value"> String to write for value </param>
        public void WriteValue(string valName, string value)
        {
            lock (this.LockObject)
            {
                Stream f = null;
                try
                {
                    var serializer = new SharpSerializer();
                    var config = new Hashtable();
                    if (File.Exists(GetConfigPath()))
                    {
                        if (OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
                        {
                            config = (Hashtable)serializer.Deserialize(f);
                        }
                    }
                    else
                    {
                        OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f);
                    }
                    config[valName] = value;
                    f.SetLength(0);
                    serializer.Serialize(config, f);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("[SimpleConfig]WriteValue Error: " + e.Message);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
            }
        }

        public IEnumerable<NamedUrl> GetFeeds()
        {
            Stream stream = null;
            while (!OpenFile(Paths.Get().FeedListPath, FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            using (var sr = new StreamReader(stream))
            {
                var lines = sr.ReadToEnd()
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x=>!String.IsNullOrWhiteSpace(x.Trim()))
                    .Select(x=>x.Split(new[]{(char)1},StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Length != 2 ? null : new NamedUrl(x[0].Trim(), x[1].Trim()))
                    .Where(x=>x != null).ToList();
                lines.Add(new NamedUrl("OCTGN Official", Paths.Get().MainOctgnFeed));
                lines.Add(new NamedUrl("Local", Paths.Get().LocalFeedPath));
                return lines;
            }
        }

        /// <summary>
        /// Adds a feed url to the feeds file. This method does not check the validity
        /// of the feed, you should use GameFeedManager.AddFeed instead
        /// </summary>
        /// <param name="feed">Feed url</param>
        public void AddFeed(NamedUrl feed)
        {
            var lines = GetFeeds().ToList();
            if (lines.Any(x => x.Name.ToLower() == feed.Name.ToLower())) return;
            Stream stream = null;
            while (!OpenFile(Paths.Get().FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            lines.Add(feed);
            using (var sr = new StreamWriter(stream))
            {
                foreach (var f in lines)
                    sr.WriteLine(f.Name + (char)1 + f.Url);
            }
        }

        /// <summary>
        /// Remove a feed url from the feed file. This method is for internal use only, use
        /// GameFeedManager.RemoveFeed instead.
        /// </summary>
        /// <param name="feed">Feed url</param>
        public void RemoveFeed(NamedUrl feed)
        {
            var lines = GetFeeds().ToList();
            Stream stream = null;
            while (!OpenFile(Paths.Get().FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            foreach (var l in lines
                .ToArray()
                .Where(l => l.Name.ToLower() == feed.Name.ToLower()))
            {
                lines.Remove(l);
            }
            using (var sr = new StreamWriter(stream))
            {
                foreach (var f in lines)
                    sr.WriteLine(f.Name + (char)1 + f.Url);
            }
        }

        public bool OpenFile(string path, FileMode fileMode, FileShare share, TimeSpan timeout, out Stream stream)
        {
            var endTime = DateTime.Now + timeout;
            while (DateTime.Now < endTime)
            {
                try
                {
                    stream = File.Open(path, fileMode, FileAccess.ReadWrite, share);
                    return true;
                }
                catch (IOException e)
                {
                    //ignore this
                }
            }
            stream = null;
            return false;
        }
    }
}