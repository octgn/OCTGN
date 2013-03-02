namespace Client
{
    using System;

    using KellyElton.SignalR.TypeSafe.ExtensionMethods;

    using Microsoft.AspNet.SignalR.Client.Hubs;

    public class TestClient : HubConnection, IDisposable
    {
        public IHubProxy HubProxy { get; set; }

         public TestClient(string host)
             : base(host)
         {
            HubProxy = this.CreateHubProxy("TestHub", new ServerToClientComs(this));
            
         }

        public void Dispose()
        {
            this.Disconnect();
        }
    }
}