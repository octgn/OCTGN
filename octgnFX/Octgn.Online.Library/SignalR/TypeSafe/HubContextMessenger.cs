namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    public class HubContextMessenger<T>
    {
        public static HubContextMessenger<T> Get(IHubConnectionContext con)
        {
            return new HubContextMessenger<T>(con);
        }

        private readonly IHubConnectionContext context;

        public HubContextMessenger(IHubConnectionContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// All connected clients.
        /// </summary>
        public T All
        {
            get
            {
                var p = DynamicProxy<T>.Get();
                foreach (var m in typeof(T).GetMethods()) p.On(m).Calls(mi => this.Call(this.context.All, mi));
                return p.Instance;
            }
        }

        /// <summary>
        /// Returns a dynamic representation of all clients except the calling client ones specified.
        /// 
        /// </summary>
        /// <param name="exclude">A list of connection ids to exclude.</param>
        /// <returns>
        /// A dynamic representation of all clients except the calling client ones specified.
        /// </returns>
        public T AllExcept(params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            foreach (var m in typeof(T).GetMethods()) p.On(m).Calls(mi => this.Call(this.context.AllExcept(exclude), mi));
            return p.Instance;
        }

        /// <summary>
        /// Returns a dynamic representation of the connection with the specified connectionid.
        /// 
        /// </summary>
        /// <param name="connectionId">The connection id</param>
        /// <returns>
        /// A dynamic representation of the specified client.
        /// </returns>
        public T Client(string connectionId)
        {
            var p = DynamicProxy<T>.Get();
            foreach (var m in typeof(T).GetMethods()) p.On(m).Calls(mi =>
                {
                    var t = this.context;
                    var ret = this.Call(this.context.Client(connectionId), mi);
                    return ret;
                });
            return p.Instance;
        }

        /// <summary>
        /// Returns a dynamic representation of the specified group.
        /// 
        /// </summary>
        /// <param name="groupName">The name of the group</param><param name="exclude">A list of connection ids to exclude.</param>
        /// <returns>
        /// A dynamic representation of the specified group.
        /// </returns>
        public T Group(string groupName, params string[] exclude)
        {
            var p = DynamicProxy<T>.Get();
            foreach (var m in typeof(T).GetMethods()) p.On(m).Calls(mi => this.Call(this.context.Group(groupName, exclude), mi));
            return p.Instance;
        }

        private Task Call(StatefulSignalProxy p, MethodCallInfo mi)
        {
            return p.Invoke(mi.Method.Name, mi.Args);
        }

        private Task Call(ClientProxy p, MethodCallInfo mi)
        {
            return p.Invoke(mi.Method.Name, mi.Args);
        }
        
        private Task Call(SignalProxy p, MethodCallInfo mi)
        {
            return p.Invoke(mi.Method.Name, mi.Args);
        }
    }

    public static class HubContextMessengerExtensionMethods
    {
        public static HubContextMessenger<T> Send<T>(this IHubConnectionContext context)
        {
            return new HubContextMessenger<T>(context);
        }

        public static HubContextMessenger<T> Send<T>(this IHub hub)
        {
            return new HubContextMessenger<T>(hub.Clients);
        }
    }
}