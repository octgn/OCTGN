namespace KellyElton.SignalR.TypeSafe.ExtensionMethods
{
    using Microsoft.AspNet.SignalR.Client.Hubs;

    public static class HubProxyMessenegerExtensionMethods
    {
        public static HubProxyMessenger<T> Send<T>(this IHubProxy proxy)
        {
            return HubProxyMessenger<T>.Get(proxy);
        }
    }
}