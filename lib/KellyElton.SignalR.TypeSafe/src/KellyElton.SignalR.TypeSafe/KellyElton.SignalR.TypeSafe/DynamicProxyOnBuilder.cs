namespace KellyElton.SignalR.TypeSafe
{
    using System;
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
            this.List = new Dictionary<int, DynamicProxyOnBuilder>();
        }

        public void Add(int methodHash, DynamicProxyOnBuilder onBuilder)
        {
            if(this.List.ContainsKey(methodHash))
                this.Set(methodHash,onBuilder);
            else
                this.List.Add(methodHash, onBuilder);
        }

        public DynamicProxyOnBuilder Get(int methodHash)
        {
            return this.List[methodHash];
        }

        public bool TryGetValue(int methodHash, out DynamicProxyOnBuilder value)
        {
            value = null;
            if (!this.List.ContainsKey(methodHash)) return false;
            value = this.List[methodHash];
            return true;
        }

        public void Set(int methodHash, dynamic onBuilder)
        {
            this.List[methodHash] = onBuilder;
        }
    }
}