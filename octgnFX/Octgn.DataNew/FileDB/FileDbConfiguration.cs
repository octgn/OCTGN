namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FileDbConfiguration
    {
        internal string Directory { get; set; }
        internal List<ICollectionDefinition> Configurations { get; set; }
        internal ICacheProvider Cache { get; set; }

        public FileDbConfiguration()
        {
            Configurations = new List<ICollectionDefinition>();
        }

        public FileDbConfiguration SetCacheProvider(ICacheProvider cache)
        {
            Cache = cache;
            return this;
        }

        public FileDbConfiguration SetCacheProvider<T>() where T : ICacheProvider
        {
            Cache = Activator.CreateInstance<T>();
            return this;
        }

        public FileDbConfiguration SetDirectory(string directory)
        {
            Directory = directory;
            return this;
        }

        public CollectionDefinition<T> DefineCollection<T>(string name)
        {
            var coll = new CollectionDefinition<T>(this,name);
            Configurations.Add(coll);
            return coll;
        }
    }
}