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

    using Polenter.Serialization;

    public static class SimpleConfig
    {
        private static object lockObject = new Object();

        /// <summary>
        /// Special case since it's required in Octgn.Data, and Prefs can't go there
        /// </summary>
        public static string DataDirectory
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

        private static string GetPath()
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
        public static string ReadValue(string valName, string def)
        {
            lock (lockObject)
            {
                var ret = def;
                Stream f = null;
                try
                {
                    if (File.Exists(GetPath()))
                    {
                        var serializer = new SharpSerializer();
                        
                        Hashtable config = new Hashtable();
                        if (OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
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
                        File.Delete(GetPath());
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
        public static void WriteValue(string valName, string value)
        {
            lock (lockObject)
            {
                Stream f = null;
                try
                {
                    var serializer = new SharpSerializer();
                    var config = new Hashtable();
                    if (File.Exists(GetPath()))
                    {
                        if (OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f))
                        {
                            config = (Hashtable)serializer.Deserialize(f);
                        }
                    }
                    else
                    {
                        OpenFile(GetPath(), FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromSeconds(2), out f);
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

        public static IEnumerable<string> GetFeeds()
        {
            Stream stream = null;
            while (!OpenFile(Paths.FeedListPath, FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            using (var sr = new StreamReader(stream))
            {
                var lines = sr.ReadToEnd()
                    .Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x=>!String.IsNullOrWhiteSpace(x.Trim()));
                return lines;
            }
        }

        /// <summary>
        /// Adds a feed url to the feeds file. This method does not check the validity
        /// of the feed, you shoudl use GameFeedManager.AddFeed instead
        /// </summary>
        /// <param name="feed">Feed url</param>
        public static void AddFeed(string feed)
        {
            var lines = GetFeeds().ToList();
            if (lines.Any(x => x.ToLower() == feed.ToLower())) return;
            Stream stream = null;
            while (!OpenFile(Paths.FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            lines.Add(feed);
            using (var sr = new StreamWriter(stream))
            {
                foreach (var f in lines)
                    sr.WriteLine(f);
            }
        }

        /// <summary>
        /// Remove a feed url from the feed file. This method is for internal use only, use
        /// GameFeedManager.RemoveFeed instead.
        /// </summary>
        /// <param name="feed">Feed url</param>
        public static void RemoveFeed(string feed)
        {
            var lines = GetFeeds().ToList();
            if (lines.Any(x => x.ToLower() == feed.ToLower())) return;
            Stream stream = null;
            while (!OpenFile(Paths.FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays(1), out stream))
            {
                Thread.Sleep(10);
            }
            foreach (var l in lines.ToArray().Where(l => l.ToLower() == feed.ToLower()))
            {
                lines.Remove(l);
            }
            using (var sr = new StreamWriter(stream))
            {
                foreach (var f in lines)
                    sr.WriteLine(f);
            }
        }

        private static bool OpenFile(string path, FileMode fileMode, FileShare share, TimeSpan timeout, out Stream stream)
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