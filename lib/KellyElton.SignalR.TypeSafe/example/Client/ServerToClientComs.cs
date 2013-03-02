namespace Client
{
    using System;
    using System.Threading.Tasks;

    using HubServer.Coms;

    using KellyElton.SignalR.TypeSafe.ExtensionMethods;
    using Shared.Coms;

    public class ServerToClientComs : IServerToClientComs
    {
        internal TestClient Client;

        public ServerToClientComs(TestClient client)
        {
            Client = client;
        }

        public Task Hello(string helloMessage)
        {
            var ret = new Task(() =>
                {
                    Console.WriteLine("Hello: {0}", helloMessage);
                    Client.HubProxy.Send<IClientToServerComs>().Invoke().HelloBack("Thanks for sending me " + helloMessage);
                });
            ret.Start();
            return ret;
        }
    }
}