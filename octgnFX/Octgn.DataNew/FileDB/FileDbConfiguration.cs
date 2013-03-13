namespace Octgn.DataNew.FileDB
{
    using System.Collections.Generic;

    public class FileDbConfiguration
    {
        internal string Directory { get; set; }
        internal List<ICollectionDefinition> Configurations { get; set; }

        public FileDbConfiguration()
        {
            Configurations = new List<ICollectionDefinition>();
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