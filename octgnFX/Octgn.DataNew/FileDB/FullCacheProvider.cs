namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using log4net;

    public class FullCacheProvider : ICacheProvider
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal Dictionary<string, CacheObject> Cache { get; set; }
        internal readonly object CacheLocker = new object();
        public FullCacheProvider()
        {
            Cache = new Dictionary<string, CacheObject>();
        }

        public bool ObjectExistsFromPath(string path)
        {
            lock(CacheLocker)
                return Cache.ContainsKey(path);
        }

        public T GetObjectFromPath<T>(string path) where T : class
        {
            lock (CacheLocker)
            {
                if (!Cache.ContainsKey(path)) return default(T);
                var ret = Cache[path].Object as T;
                return ret;
            }
        }

        public void AddObjectToCache(string path, object obj)
        {
            lock (CacheLocker)
            {
                if (Cache.ContainsKey(path))
                {
                    Cache[path].Dispose();
                    Cache.Remove(path);
                }
                Cache.Add(path,new CacheObject(this,path,obj));
            }
        }
        public void InvalidatePath(string path)
        {
            lock (CacheLocker)
            {
                if (Cache.ContainsKey(path))
                {
                    Log.DebugFormat("Invalidating Path {0}", path);
                    Cache[path].Dispose();
                    Cache.Remove(path);
                    Log.DebugFormat("Path Invalidated {0}",path);
                }
            }
        }
        public void InvalidateObject(object obj)
        {
            lock (CacheLocker)
            {
                foreach (var o in Cache.Where(x => x.Value.Object == obj).ToArray())
                {
                    Cache[o.Key].Dispose();
                    Cache.Remove(o.Key);
                }
            }
        }
    }
    internal class CacheObject : IDisposable
    {
        internal string Path { get; set; }
        internal object Object { get; set; }
        internal FileSystemWatcher Watcher { get; set; }
        internal ICacheProvider Provider { get; set; }
        internal bool Disposed;
        public CacheObject(ICacheProvider provider, string path, object obj)
        {
            var fi = new FileInfo(path);
            Provider = provider;
            Path = path;
            Object = obj;
            Watcher = new FileSystemWatcher(fi.Directory.FullName,fi.Name);
            Watcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.Size
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName ;
            Watcher.Changed += WatcherOnChanged;
            Watcher.Deleted += WatcherOnDeleted;
            Watcher.Renamed += WatcherOnRenamed;
            Watcher.EnableRaisingEvents = true;
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            Provider.InvalidatePath(Path);
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Provider.InvalidatePath(Path);
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Provider.InvalidatePath(Path);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Watcher.EnableRaisingEvents = false;
            Watcher.Changed -= this.WatcherOnChanged;
            Watcher.Deleted -= this.WatcherOnDeleted;
            Watcher.Renamed -= this.WatcherOnRenamed;
            Disposed = true;
            Watcher.Dispose();
            Object = null;
            Path = null;
        }
    }
}