namespace KellyElton.SignalR.TypeSafe
{
    using Microsoft.AspNet.SignalR.Client.Hubs;

    public class HubProxyMessenger<T>
    {
        public static HubProxyMessenger<T> Get(IHubProxy proxy)
        {
            return new HubProxyMessenger<T>(proxy);
        }

        private readonly IHubProxy proxy;

        public HubProxyMessenger(IHubProxy proxy)
        {
            this.proxy = proxy;
        }

        public T Invoke()
        {
            var p = DynamicProxy<T>.Get();
            foreach(var m in typeof(T).GetMethods())
                p.On(m).Calls(mi => this.proxy.Invoke(mi.Method.Name, mi.Args));
            return p.Instance;
        }
    }
}