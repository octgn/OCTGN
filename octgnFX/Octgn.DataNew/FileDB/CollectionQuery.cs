namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    using Octgn.Library.ExtensionMethods;

    public class CollectionQuery<T>
    {
        internal List<DirectoryInfo> Index { get; set; }
        internal ICollectionDefinition Def { get; set; }
        public Type ElementType { get { return typeof(T); } }
        private readonly object indexLock = new object();

        public CollectionQuery(ICollectionDefinition def)
        {
            this.Def = def;
            this.CreateSearchIndex();
        }

        internal void CreateSearchIndex()
        {
            lock (indexLock)
            {
                var config = Def;
                var root = new DirectoryInfo(Path.Combine(Def.Config.Directory, config.Root.PartString()));
                foreach (var r in root.SplitFull().Where(r => !Directory.Exists(r.FullName))) Directory.CreateDirectory(r.FullName);

                this.Index = new List<DirectoryInfo>();

                this.Index.Add(root);
                var done = false;
                foreach (var part in config.Parts)
                {
                    if (done) break;
                    switch (part.PartType)
                    {
                        case PartType.Directory:
                            for (var i = 0; i < this.Index.Count; i++)
                            {
                                this.Index[i] = new DirectoryInfo(Path.Combine(this.Index[i].FullName, part.PartString()));
                                if (!Directory.Exists(this.Index[i].FullName)) Directory.CreateDirectory(this.Index[i].FullName);
                            }
                            break;
                        case PartType.Property:
                            //if next not file
                            var newList = new List<DirectoryInfo>();
                            foreach (var i in this.Index)
                            {
                                newList.AddRange(i.GetDirectories());
                            }
                            this.Index = newList;
                            break;
                        case PartType.File:
                            foreach (var item in this.Index.Where(i => i.GetFiles().Any(x => x.Name == part.PartString()) == false).ToArray())
                            {
                                this.Index.Remove(item);
                            }
                            done = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public CollectionQuery<T> By<P>(Expression<Func<T, P>> property, Op op, P value)
        {
            var queryPart = new Part<T>();
            queryPart.Property(property);

            if (Def.Parts.Any(x => x.PartString() == queryPart.PartString()) == false)
                throw new ArgumentException("There is no property " + queryPart.ToString() + " defined for the collection " + Def.Name, "property");

            var partIndex = 0;
            foreach (var part in new DirectoryInfo(Def.Path).Split())
            {
                if (part != queryPart.PartString())
                {
                    partIndex++;
                    continue;
                }
                foreach (var i in Index.ToArray())
                {
                    var dirParts = i.Split();
                    var partString = dirParts[partIndex];
                    if (!partString.Is(queryPart.Type))
                    {
                        Index.Remove(i);
                        continue;
                    }
                    switch (op)
                    {
                        case Op.Eq:
                            if (partString == value.ToString())continue;
                            break;
                        case Op.Neq:
                            if (partString != value.ToString()) continue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("op");
                    }
                    Index.Remove(i);
                }
                break;
            }
            return this;
        }
    }

    public enum Op
    {
        Eq, Neq
    }
}