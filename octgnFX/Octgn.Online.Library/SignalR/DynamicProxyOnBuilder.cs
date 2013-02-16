namespace Octgn.Online.Library.SignalR
{
    using System;

    public class DynamicProxyOnBuilder
    {
        internal Action ThisCalls = () => { };

        public DynamicProxyOnBuilder Calls(Action action)
        {
            ThisCalls = action;
            return this;
        }
    }
}