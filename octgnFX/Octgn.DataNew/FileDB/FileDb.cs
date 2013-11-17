namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Octgn.Library.Exceptions;

    using log4net;

    public class FileDb
    {
        internal FileDbConfiguration Config { get; set; }

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public FileDb(FileDbConfiguration config)
        {
            Config = config;
        }

        public CollectionQuery<T> Query<T>() where T : class
        {
            try
            {
                if (Config == null || Config.Configurations == null) throw new UserMessageException("Stop goofing around!");
                var config = Config.Configurations.Where(x => x.Type == typeof(T)).ToList();
                if (config.Count == 0) throw new ArgumentException("can't find definition for type " + typeof(T).Name, "T");

                return new CollectionQuery<T>(config);

            }
            catch (Exception e)
            {
                throw new UserMessageException(
                    "There is something wrong with your database. This may be caused by a broken game. If you are unable to uninstall the game, please let us know.");
            }
        }
    }
    //internal class FileDbFsWatcherCollection
    //{
    //    internal Dictionary<string, FileSystemWatcher> Watchers { get; set; }

    //    internal FileDbFsWatcherCollection()
    //    {
    //        Watchers = new Dictionary<string, FileSystemWatcher>();
    //    }

    //    internal void Add(string path)
    //    {
    //        if (Watchers.ContainsKey(path)) return;
    //        var watch = new FileSystemWatcher(,);
    //        watch.Changed += WatchOnChanged;
    //    }

    //    private void WatchOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
    //    {
            
    //    }
    //}
    //internal class Asdf : FileSystemWatcher
    //{
    //    override 
    //}
}