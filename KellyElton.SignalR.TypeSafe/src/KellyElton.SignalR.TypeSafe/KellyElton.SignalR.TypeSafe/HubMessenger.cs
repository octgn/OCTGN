namespace KellyElton.SignalR.TypeSafe
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR.Hubs;

    public class HubMessenger<T>
    {
        public static HubMessenger<T> Get(HubConnectionContext con)
        {
            return new HubMessenger<T>(con);
        }

        private readonly HubConnectionContext context;

        public HubMessenger(HubConnectionContext context)
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
                foreach (var m in typeof(T).GetMethods())
                    p.On(m).Calls(mi => this.Call(this.context.All, mi));
                return p.Instance;
            }
        }

        /// <summary>
        /// Represents the calling client.
        /// 
        /// </summary>
        public T Caller
        {
            get
            {
                var p = DynamicProxy<T>.Get();
                foreach (var m in typeof(T).GetMethods())
                    p.On(m).Calls(mi => this.Call(this.context.Caller, mi));
                return p.Instance;
            }
        }

        /// <summary>
        /// All connected clients except the calling client.
        /// 
        /// </summary>
        public T Others
        {
            get
            {
                var p = DynamicProxy<T>.Get();
                foreach (var m in typeof(T).GetMethods())
                    p.On(m).Calls(mi => this.Call(this.context.Others, mi));
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
            foreach (var m in typeof(T).GetMethods())
                p.On(m).Calls(mi => this.Call(this.context.AllExcept(exclude), mi));
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
            foreach (var m in typeof(T).GetMethods())
                p.On(m).Calls(mi => this.Call(this.context.Client(connectionId), mi));
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
            foreach (var m in typeof(T).GetMethods())
                p.On(m).Calls(mi => this.Call(this.context.Group(groupName, exclude), mi));
            return p.Instance;
        }

        /// <summary>
        /// Returns a dynamic representation of all clients in a group except the calling client.
        /// 
        /// </summary>
        /// <param name="groupName">The name of the group</param>
        /// <returns>
        /// A dynamic representation of all clients in a group except the calling client.
        /// </returns>
        public T OthersInGroup(string groupName)
        {
            var p = DynamicProxy<T>.Get();
            foreach (var m in typeof(T).GetMethods())
                p.On(m).Calls(mi => this.Call(this.context.OthersInGroup(groupName), mi));
            return p.Instance;
        }

        private Task Call(StatefulSignalProxy p, MethodCallInfo mi)
        {
           return p.Invoke(mi.Method.Name, mi.Args);
        }

        private Task Call(ClientProxy p, MethodCallInfo mi)
        {
            return p.Invoke(mi.Method.Name, mi.Args );
        }
    }
}