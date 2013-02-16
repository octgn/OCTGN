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
                p.OnAll().Calls(() => this.Call(context.All));
                return p.Instance;
            }
        }

        public T AllExcept(params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls(()=>this.Call(context.AllExcept(exclude)));
            return p.Instance;
        }

        public T Client(string connectionId)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls(() => this.Call(context.Client(connectionId)));
            return p.Instance;
        }

        public T Group(string groupName, params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            p.OnAll().Calls(() => this.Call(context.Group(groupName, exclude)));
            return p.Instance;
        }

        private void Call(StatefulSignalProxy p)
        {
            // TODO I need to get method info a params
            var frames = new StackTrace().GetFrames();
            Debugger.Break();
        }

        private void Call(ClientProxy p)
        {
            // TODO I need to get method info a params
            var frames = new StackTrace().GetFrames();
            var mi = frames[5].GetMethod();
            //p.Invoke(mi.Name,new object[]{mi.a})
            Debugger.Break();
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