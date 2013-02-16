namespace Octgn.Online.Library.SignalR
{
    using System;

    public class DynamicProxyOnBuilder
    {
        internal Action<MethodCallInfo> ThisCalls = (asdf) => { };

        public DynamicProxyOnBuilder Calls(Action<MethodCallInfo> action)
        {
            ThisCalls = action;
            return this;
        }
    }
}