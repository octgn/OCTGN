namespace Octgn.Online.Library.SignalR
{
    using System.Diagnostics;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public class HubMessenger<T>
    {
        public static HubMessenger<T> Get(IHubConnectionContext con)
        {
            return new HubMessenger<T>(con);
        }

        private IHubConnectionContext context;

        public HubMessenger(IHubConnectionContext context)
        {
            this.context = context;
        }

        public T All
        {
            get
            {
                var p = DynamicProxy<T>.Get();
                p.OnAll().Calls((mi) => this.Call(context.All, mi));
                return p.Instance;
            }
        }

        public T AllExcept(params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls((mi) => this.Call(context.AllExcept(exclude),mi));
            return p.Instance;
        }

        public T Client(string connectionId)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls((mi) => this.Call(context.Client(connectionId), mi));
            return p.Instance;
        }

        public T Group(string groupName, params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls((mi) => this.Call(context.Group(groupName, exclude), mi));
            return p.Instance;
        }

        private void Call(StatefulSignalProxy p, MethodCallInfo mi)
        {
            // TODO I need to get method info a params
            p.Invoke(mi.Method.Name, mi.Args);
        }

        private void Call(ClientProxy p, MethodCallInfo mi)
        {
            // TODO I need to get method info a params
            p.Invoke(mi.Method.Name, mi.Args );
        }
    }
    public static class HubMessengerExtensionMethods
    {
        public static HubMessenger<T> Send<T>(this HubConnectionContext context)
        {
            return new HubMessenger<T>(context);
        }
        public static HubMessenger<T> Send<T>(this Hub hub)
        {
            return new HubMessenger<T>(hub.Clients);
        }
    }
}