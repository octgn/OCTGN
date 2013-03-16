namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class FileDb
    {
        internal FileDbConfiguration Config { get; set; }
        public FileDb(FileDbConfiguration config)
        {
            Config = config;
        }

        public CollectionQuery<T> Query<T>() where T : class
        {
            var config = Config.Configurations.FirstOrDefault(x => x.Type == typeof(T));
            if (config == null)
                throw new ArgumentException("can't find definition for type " + typeof(T).Name, "T");

            return new CollectionQuery<T>(config);
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