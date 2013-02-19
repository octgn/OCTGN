namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System.Collections.Generic;

    using Microsoft.AspNet.SignalR.Client.Hubs;

    public static class HubConnectionExtensionMethods
    {
        public static IHubProxy CreateHubProxy(this HubConnection connection, string hub, object obj)
        {
            var proxy = connection.CreateHubProxy(hub);
            var theType = obj.GetType();
            foreach (var method in theType.GetMethods())
            {
                var method1 = method;
                var sub = proxy.Subscribe(method1.Name);
                sub.Data += tokens =>
                {
                    var args = new List<object>();
                    var curArg = 0;
                    foreach (var tok in tokens)
                    {
                        args.Add(tok.ToObject(method1.GetParameters()[curArg].ParameterType));
                        curArg++;
                    }
                    method1.Invoke(obj, args.ToArray());
                };
            }
            return proxy;
        }
    }
}