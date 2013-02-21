namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class DynamicProxyOnBuilder
    {
        public Delegate ThisCalls { get; set; }
        public Type ReturnType { get; set; }

        public DynamicProxyOnBuilder Calls(Action<MethodCallInfo> action)
        {
            this.ThisCalls = action;
            return this;
        }

        public DynamicProxyOnBuilder Calls<T>(Func<MethodCallInfo, T> action)
        {
            this.ThisCalls = action;
            return this;
        }
    }

    public class DynamicProxyOnBuilderList
    {
        internal Dictionary<int, DynamicProxyOnBuilder> List { get; set; } 

        public DynamicProxyOnBuilderList() 
        {
            List = new Dictionary<int, DynamicProxyOnBuilder>();
        }

        public void Add(int methodHash, DynamicProxyOnBuilder onBuilder)
        {
            if(List.ContainsKey(methodHash))
                this.Set(methodHash,onBuilder);
            else
                List.Add(methodHash, onBuilder);
        }

        public DynamicProxyOnBuilder Get(int methodHash)
        {
            return List[methodHash];
        }

        public bool TryGetValue(int methodHash, out DynamicProxyOnBuilder value)
        {
            value = null;
            if (!List.ContainsKey(methodHash)) return false;
            value = List[methodHash];
            return true;
        }

        public void Set(int methodHash, dynamic onBuilder)
        {
            List[methodHash] = onBuilder;
        }
    }
}