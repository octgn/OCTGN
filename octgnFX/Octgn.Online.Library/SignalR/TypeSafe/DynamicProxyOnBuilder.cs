namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System;

    public class DynamicProxyOnBuilder
    {
        internal Action<MethodCallInfo> ThisCalls = asdf => { };

        public DynamicProxyOnBuilder Calls(Action<MethodCallInfo> action)
        {
            this.ThisCalls = action;
            return this;
        }
    }
}