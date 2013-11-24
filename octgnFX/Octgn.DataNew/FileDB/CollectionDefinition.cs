namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    using Octgn.Library.ExtensionMethods;

    public interface ICollectionDefinition
    {
        bool IsSteril { get; }
        Guid Key { get; }
        Type Type { get; }
        string Name { get; }
        IEnumerable<IPart> Parts { get; }
        IFileDbSerializer Serializer { get; }
        IPart Root { get; }
        FileDbConfiguration Config { get; }
        string Path { get; }

        List<DirectoryInfo> CreateSearchIndex();
    }

    public class CollectionDefinition<T> : ICollectionDefinition
    {
        public bool IsSteril { get; internal set; }
        public Guid Key { get; internal set; }
        public Type Type { get; internal set; }
        public string Name { get; internal set; }
        public IEnumerable<IPart> Parts { get; internal set; }
        public IFileDbSerializer Serializer { get; internal set; }
        public IPart Root { get; internal set; }
        public FileDbConfiguration Config { get; internal set; }
        public string Path
        {
            get
            {
                var dir = new System.IO.DirectoryInfo(Config.Directory);
                var ret = System.IO.Path.Combine(dir.FullName, Root.PartString());
                return this.Parts.Aggregate(ret, (current, p) => System.IO.Path.Combine(current, p.PartString()));
            }
        }

        public CollectionDefinition(FileDbConfiguration config, string name)
        {
            Key = Guid.NewGuid();
            Config = config;
            Name = name;
            Root = new Part<T>().Directory(name);
            Parts = new List<IPart>();
            Type = typeof(T);
            //TODO [DB MIGRATION] Setting the file should only happen once, so it shouldn't be a part of the Parts enumerable
        }
        public CollectionDefinition<T> SetSteril()
        {
            IsSteril = true;
            return this;
        }
        public FileDbConfiguration Conf()
        {
            return Config;
        }
        public CollectionDefinition<T> OverrideRoot(Expression<Func<Part<T>, Part<T>>> part)
        {
            var body = part.Body as MethodCallExpression;
            var npart = new Part<T>();
            var args = new List<object>();
            foreach (var a in body.Arguments)
            {
                if (a is ConstantExpression)
                    args.Add((a as ConstantExpression).Value);
                else if (a is UnaryExpression)
                    args.Add((a as UnaryExpression).Operand);
                else throw new NotImplementedException();
            }
            var res = body.Method.Invoke(npart, args.ToArray()) as Part<T>;
            if (res.PartType != PartType.Directory) throw new ArgumentException("Root must be of type Directory, not " + res.PartType.ToString(), "part");
            Root = res;
            return this;
        }
        public CollectionDefinition<T> SetPart(Expression<Func<Part<T>, Part<T>>> part)
        {
            var body = part.Body as MethodCallExpression;
            var npart = new Part<T>();
            var args = new List<object>();
            foreach (var a in body.Arguments)
            {
                if (a is ConstantExpression)
                    args.Add((a as ConstantExpression).Value);
                else if (a is UnaryExpression)
                    args.Add((a as UnaryExpression).Operand);
                else if (a is MethodCallExpression)
                {
                    args.Add(Expression.Lambda((a as MethodCallExpression)).Compile().DynamicInvoke());
                }
                else if (a is BinaryExpression)
                {
                    var l = (a as BinaryExpression);
                    args.Add(Expression.Lambda(l).Compile().DynamicInvoke());
                }
                else throw new NotImplementedException(a.GetType().Name);
            }
            var res = body.Method.Invoke(npart, args.ToArray()) as Part<T>;
            (Parts as List<IPart>).Add(res);
            return this;
        }
        public CollectionDefinition<T> SetSerializer<ST>()  where ST : IFileDbSerializer
        {
            this.Serializer = Activator.CreateInstance<ST>();
            this.Serializer.Def = this;
            return this;
        }
        public CollectionDefinition<T> SetSerializer(IFileDbSerializer serializer)
        {
            this.Serializer = serializer;
            if (serializer.Def == null) serializer.Def = this;
            return this;
        }
        public List<DirectoryInfo> CreateSearchIndex()
        {
            var root = new DirectoryInfo(System.IO.Path.Combine(Config.Directory,Root.PartString()));
			root.Create();

            var ret = new List<DirectoryInfo>();

            ret.Add(root);
            var done = false;
            foreach (var part in Parts)
            {
                if (done) break;
                switch (part.PartType)
                {
                    case PartType.Directory:
                        for (var i = 0; i < ret.Count; i++)
                        {
                            ret[i] = new DirectoryInfo(System.IO.Path.Combine(ret[i].FullName, part.PartString()));
                            if (!Directory.Exists(ret[i].FullName)) Directory.CreateDirectory(ret[i].FullName);
                        }
                        break;
                    case PartType.Property:
                        //if next not file
                        var newList = new List<DirectoryInfo>();
                        foreach (var i in ret)
                        {
                            newList.AddRange(i.GetDirectories());
                        }
                        ret = newList;
                        break;
                    case PartType.File:
                        foreach(var item in ret.Where(i => i.GetFiles(part.PartString()).Any() == false).ToArray())
                        {
                            ret.Remove(item);
                        }
                        done = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return ret;
        }
    }
}