namespace Octgn.Online.Library.SignalR.TypeSafe
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
            p.OnAll().Calls(mi => this.proxy.Invoke(mi.Method.Name, mi.Args));
            return p.Instance;
        }
    }

    public static class HubProxyMessenegerExtensionMethods
    {
        public static HubProxyMessenger<T> Send<T>(this IHubProxy proxy)
        {
            return HubProxyMessenger<T>.Get(proxy);
        }
    }
}