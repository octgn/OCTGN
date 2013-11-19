using System.Text.RegularExpressions;
using Octgn.Library.ExtensionMethods;

namespace Octgn.Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Octgn.Library.Exceptions;
    using Octgn.Library.Networking;

    using Polenter.Serialization;

    using log4net;

    public interface ISimpleConfig
    {
        string DataDirectory { get; set; }
        string GetConfigPath();

        T ReadValue<T>(string valName, T def);

        void WriteValue<T>(string valName, T value);

        IEnumerable<NamedUrl> GetFeeds(bool localOnly = false);

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
            this.Init();
        }

        #endregion Singleton

        internal object LockObject = new Object();

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        internal void Init()
        {
        }

        public string GetConfigPath()
        {
            string p = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
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
        public T ReadValue<T>(string valName, T def)
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
                        if (OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(20), out f))
                        {
                            // Fix for https://github.com/kellyelton/OCTGN/issues/1116
                            //   This will make sure that even if a <null/> creeps in there it doesn't care about it
                            {
                                string configString = "";
                                using (var sr = new StreamReader(f))
                                {
                                    configString = sr.ReadToEnd();

                                    configString = Regex.Replace(configString, "<[ ]*null[ ]*/>", "",
                                        RegexOptions.Multiline | RegexOptions.IgnoreCase);

                                    using (var stringstream = configString.ToStream())
                                    {
                                        config = (Hashtable)serializer.Deserialize(stringstream);
                                    }
                                }
                            }
                        }
                        if (config.ContainsKey(valName))
                        {
                            if (config[valName] is T)
                            {
                                return (T)config[valName];
                            }
                            else
                            {
                                var conv = TypeDescriptor.GetConverter(typeof(T));
                                var val = (T)conv.ConvertFromInvariantString(config[valName].ToString());
                                config[valName] = val;
                                f.SetLength(0);
                                serializer.Serialize(config, f);
                                return val;
                            }
                        }
                        else
                        {
                            config[valName] = def;
                            f.SetLength(0);
                            serializer.Serialize(config, f);
                            return def;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("ReadValue Error", e);
                    try
                    {
                        File.Delete(GetConfigPath());
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("SReadValue Error: Couldn't delete the corrupt config file.",ex);
                        throw new UserMessageException(
                            "There was an error reading your config file. Please exit all instances of OCTGN and re open it to fix this problem. You may also need to restart your pc.");
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
        public void WriteValue<T>(string valName, T value)
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
                        if (OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(20), out f))
                        {
                            // Fix for https://github.com/kellyelton/OCTGN/issues/1116
                            //   This will make sure that even if a <null/> creeps in there it doesn't care about it
                            {
                                string configString = "";
                                using (var sr = new StreamReader(f))
                                {
                                    configString = sr.ReadToEnd();

                                    configString = Regex.Replace(configString, "<[ ]*null[ ]*/>", "",
                                        RegexOptions.Multiline | RegexOptions.IgnoreCase);

                                    using (var stringstream = configString.ToStream())
                                    {
                                        config = (Hashtable)serializer.Deserialize(stringstream);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        OpenFile(GetConfigPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(20), out f);
                    }
                    // Fix for https://github.com/kellyelton/OCTGN/issues/1116
                    {
                        if (typeof (T).IsValueType)
                        {
                            config[valName] = value;
                            Console.WriteLine("value=" + value);
                        }
                        else if (value == null)
                        {
                            config[valName] = string.Empty;
                        }
                        else
                            config[valName] = value;
                    }
                    f.SetLength(0);
                    serializer.Serialize(config, f);
                }
                catch (Exception e)
                {
                    Log.Warn("WriteValue Error", e);
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

        public IEnumerable<NamedUrl> GetFeeds(bool localOnly = false)
        {
            var ret = new List<NamedUrl>();
            ret.Add(new NamedUrl("Local", Paths.Get().LocalFeedPath));
            if (!localOnly)
            {
                ret.Add(new NamedUrl("OCTGN Official", Paths.Get().MainOctgnFeed));
                ret.AddRange(this.GetFeedsList().ToList());
            }
            return ret;
        }

        internal IEnumerable<NamedUrl> GetFeedsList()
        {
            Stream stream = null;
            var wasLocked = false;
            while (!OpenFile(Paths.Get().FeedListPath, FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromDays(1), out stream))
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
            var lines = GetFeedsList().ToList();

            // correct myGet URLS -- correct them both here before the check to make sure we don't get an http and https version of the same.
            feed.Url = CorrectMyGetFeed(feed.Url);
            lines.ForEach(line => line.Url = CorrectMyGetFeed(line.Url));

            if (lines.Any(x => x.Name.ToLower() == feed.Name.ToLower())) return;

            lines.Add(feed);

            Stream stream = null;
            while (!OpenFile(Paths.Get().FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            using (var sr = new StreamWriter(stream))
            {
                lines.ForEach(line => sr.WriteLine(line.Name + (char)1 + line.Url));
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


        /// <summary>
        /// Remove a feed url from the feed file. This method is for internal use only, use
        /// GameFeedManager.RemoveFeed instead.
        /// </summary>
        /// <param name="feed">Feed url</param>
        public void RemoveFeed(NamedUrl feed)
        {
            var lines = GetFeedsList().ToList();
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
            //Log.DebugFormat("Open file {0} {1} {2} {3}", path, fileMode, share, timeout.ToString());
            var endTime = DateTime.Now + timeout;
            while (DateTime.Now < endTime)
            {
                //Log.DebugFormat("Trying to lock file {0}", path);
                try
                {
                    stream = File.Open(path, fileMode, FileAccess.ReadWrite, share);
                    //Log.DebugFormat("Got lock on file {0}", path);
                    return true;
                }
                catch (IOException e)
                {
                    Log.Warn("Could not acquire lock on file " + path, e);
                }
            }
            //Log.WarnFormat("Timed out reading file {0}", path);
            stream = null;
            return false;
        }
    }
}