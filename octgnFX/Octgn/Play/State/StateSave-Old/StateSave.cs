namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Serialization.Formatters.Binary;

    [Serializable]
    [Obsolete]
    public abstract class StateSave<T> : IStateSave where T : class
    {
        internal static Type[] StateSaveTypes;

        private static readonly object stateSaveTypesLocker = new object();
        
        public static StateSave<T1> Create<T1>(T1 instance) where T1 : class
        {
            if (StateSaveTypes == null)
            {
                lock (stateSaveTypesLocker)
                {
                    if (StateSaveTypes == null)
                    {
                        var l = new List<Type>();
                        var asses = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var a in asses)
                        {
                            var add =
                                a.GetTypes()
                                 .Where(
                                     x => x.GetInterfaces().Any(y => y == typeof(IStateSave)) && x.IsAbstract == false)
                                 .ToArray();
                            l.AddRange(add);
                        }
                        StateSaveTypes = l.ToArray();
                    }
                }
            }

            var sst = StateSaveTypes.FirstOrDefault(x => x.IsSubclassOf(typeof(StateSave<T1>)));
            if (sst == null)
                throw new InvalidOperationException("No StateSave<T> Found for Type " + typeof(T).Name);
            return Activator.CreateInstance(sst, instance) as StateSave<T1>;
        }

        protected StateSave(T instance)
        {
            this.Instance = instance;
            this.Values = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Values { get; private set; }

        [NonSerialized]
        protected T Instance;

        public abstract void SaveState();

        public abstract void LoadState();

        public void SetInstance(object obj)
        {
            this.Instance = obj as T;
        }

        public object GetInstance()
        {
            return this.Instance;
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                var br = new BinaryFormatter();
                br.Serialize(ms, this);
                ms.Flush();
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        protected void Save<P>(Expression<Func<T, P>> prop)
        {
            var propstr = this.GetPropString(prop);
            this.Values.Add(propstr, prop.Compile().Invoke(this.Instance));
        }

        protected void Save<P>(Expression<Func<T, P>> prop, object value)
        {
            var propstr = this.GetPropString(prop);
            this.Values.Add(propstr, value);
        }

        protected void Load<P>(Expression<Func<T, P>> prop)
        {
            var propstr = this.GetPropString(prop);
            var val = this.Values[propstr];
            typeof(T).GetField(propstr);
            var p = typeof(T).GetProperty(propstr);
            if (p != null)
            {
                p.SetValue(this.Instance, val, null);
            }
            else
            {
                var f = typeof(T).GetField(propstr);
                f.SetValue(this.Instance,val);
            }
        }

        protected void Load<P, C>(Expression<Func<T, P>> prop, Func<T, C,P> set)
        {
            var propstr = this.GetPropString(prop);
            var val = (C)this.Values[propstr];
            var setVal = set.Invoke(this.Instance, val);
            var p = typeof(T).GetProperty(propstr);
            if (p != null)
            {
                p.SetValue(this.Instance, setVal, null);
            }
            else
            {
                var f = typeof(T).GetField(propstr);
                f.SetValue(this.Instance, setVal);
            }
            
        }

        protected P LoadAndReturn<P>(Expression<Func<T, P>> prop)
        {
            var propstr = this.GetPropString(prop);
            var val = this.Values[propstr];
            return (P)val;
        }

        protected R LoadAndReturn<P,R>(Expression<Func<T, P>> prop)
        {
            var propstr = this.GetPropString(prop);
            var val = this.Values[propstr];
            return (R)val;
        }

        private string GetPropString<P>(Expression<Func<T, P>> prop)
        {
            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Must be set from a field or property.", "prop");
            var exp = (prop.Body as MemberExpression);
            var plist = new List<String>();
            while (exp != null)
            {
                plist.Add(exp.Member.Name);
                exp = exp.Expression as MemberExpression;
            }
            var propstr = String.Join(".", plist);
            return propstr;
        }
    }
}