namespace KellyElton.SignalR.TypeSafe.ExtensionMethods
{
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

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