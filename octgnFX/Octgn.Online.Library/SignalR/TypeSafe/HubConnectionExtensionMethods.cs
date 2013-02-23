namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System.Collections.Generic;

    using Microsoft.AspNet.SignalR.Client.Hubs;

    using Octgn.Online.Library.SignalR.TypeSafe.ExtensionMethods;

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
                sub.Received += tokens =>
                {
                    var args = new List<object>();
                    var curArg = 0;
                    foreach (var tok in tokens)
                    {
                        var p = method1.GetParameters()[curArg];
                        // "Because if it's not a reference type variable, it needs an explicit cast" 
                        // Quote of the day brought to you by Kelly Elton Inc, where Chocolate rains on the clouds of the innocent.
                        if (method1.GetParameters()[curArg].ParameterType.IsSimpleType())
                        {
                            args.Add(tok.Cast(method1.GetParameters()[curArg].ParameterType));
                        }
                        else
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