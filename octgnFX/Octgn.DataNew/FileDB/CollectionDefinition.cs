namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface ICollectionDefinition
    {
        string Name { get; }
        IEnumerable<IPart> Parts { get; }
        IPart Root { get; }
        FileDbConfiguration Config { get; }
        string Path { get; }
    }

    public class CollectionDefinition<T> : ICollectionDefinition
    {
        public string Name { get; internal set; }
        public IEnumerable<IPart> Parts { get; internal set; }
        public IPart Root { get; internal set; }
        public FileDbConfiguration Config { get; internal set; }
        public string Path{get
        {
            var dir = new System.IO.DirectoryInfo(Config.Directory);
            var ret = System.IO.Path.Combine(dir.FullName,Root.PartString());
            return this.Parts.Aggregate(ret, (current, p) => System.IO.Path.Combine(current, p.PartString()));
        }}
        public CollectionDefinition(FileDbConfiguration config, string name)
        {
            Config = config;
            Name = name;
            Root = new Part<T>().Directory(name);
            Parts = new List<IPart>();
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
            Root = res;
            return this;
        }
        public CollectionDefinition<T> SetPart(Expression<Func<Part<T>,Part<T>>> part)
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
            var res = body.Method.Invoke(npart, args.ToArray())as Part<T>;
            (Parts as List<IPart>).Add(res);
            return this;
        }
    }
}