namespace Octgn.Online.Library.SignalR
{
    using System.Collections.Generic;

    using Microsoft.AspNet.SignalR.Client.Hubs;

    public class Client2 : HubConnection
    {
        public Client2(string url)
            : base(url)
        {
        }

        public Client2(string url, bool useDefaultUrl)
            : base(url, useDefaultUrl)
        {
        }

        public Client2(string url, string queryString)
            : base(url, queryString)
        {
        }

        public Client2(string url, string queryString, bool useDefaultUrl)
            : base(url, queryString, useDefaultUrl)
        {
        }

        public Client2(string url, IDictionary<string, string> queryString)
            : base(url, queryString)
        {
        }

        public Client2(string url, IDictionary<string, string> queryString, bool useDefaultUrl)
            : base(url, queryString, useDefaultUrl)
        {
        }
    }
    public static class Client2ExtensionMethods
    {
        
    }
}