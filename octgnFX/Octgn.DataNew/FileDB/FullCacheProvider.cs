namespace Octgn.DataNew.FileDB
{
    using System.Collections.Generic;

    public class FullCacheProvider : ICacheProvider
    {
        internal Dictionary<string, object> Cache { get; set; }

        public FullCacheProvider()
        {
            Cache = new Dictionary<string, object>();
        }

        public bool ObjectExistsFromPath(string path)
        {
            return Cache.ContainsKey(path);
        }

        public T GetObjectFromPath<T>(string path) where T : class
        {
            if (!Cache.ContainsKey(path)) return default(T);
            var ret = Cache[path] as T;
            return ret;
        }

        public void AddObjectToCache(string path, object obj)
        {
            if (Cache.ContainsKey(path)) Cache[path] = obj;
            else
            {
                Cache.Add(path,obj);
            }
        }
    }
}