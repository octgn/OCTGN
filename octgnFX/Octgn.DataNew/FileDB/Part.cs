namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class Part<T> : IPart
    {
        public PartType PartType { get; internal set; }
        public Type Type { get; internal set; }

        public string ThisPart { get; internal set; }

        public Part<T> Directory(string name)
        {
            this.ThisPart = name;
            this.PartType = PartType.Directory;
            return this;
        }
        public Part<T> Property<P>(Expression<Func<T, P>> property)
        {
            if(!(property.Body is MemberExpression))
                throw new ArgumentException("Must be set from a field or property.","property");
            var exp = (property.Body as MemberExpression);
            var plist = new List<String>();
            while (exp != null)
            {
                plist.Add(exp.Member.Name);
                exp = exp.Expression as MemberExpression;
            }
            plist.Reverse();
            this.ThisPart = String.Join(".", plist);
            this.PartType = PartType.Property;
            this.Type = typeof(P);
            return this;
        }
        public Part<T> File(string name)
        {
            this.ThisPart = name;
            this.PartType = PartType.File;
            return this;
        }
        public string PartString()
        {
            switch (this.PartType)
            {
                case PartType.Directory:
                    return this.ThisPart as String;
                case PartType.Property:
                    return "{" + this.ThisPart + "}";
                case PartType.File:
                    return this.ThisPart as String;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}