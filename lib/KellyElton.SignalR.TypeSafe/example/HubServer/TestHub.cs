namespace HubServer
{
    using System;
    using System.Threading.Tasks;

    using KellyElton.SignalR.TypeSafe;

    using HubServer.Coms;

    using KellyElton.SignalR.TypeSafe.ExtensionMethods;

    using Microsoft.AspNet.SignalR;

    using Shared.Coms;

    public class TestHub : Hub, IClientToServerComs
    {
        #region Connection Events
        public override Task OnConnected()
        {
            Console.WriteLine("Connected {0}", this.Context.ConnectionId);

            // On someone connecting, write them a message!
            this.Clients.Send<IServerToClientComs>().Caller.Hello("Hello Guy!");

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            Console.WriteLine("Disconnected {0}", this.Context.ConnectionId);
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            Console.WriteLine("Reconnected {0}", this.Context.ConnectionId);

            // Welcome them back
            this.Clients.Send<IServerToClientComs>().Caller.Hello("Welcome back!");

            return base.OnReconnected();
        }
        #endregion

        public Task HelloBack(string returnMessage)
        {
            var ret = new Task(() => Console.WriteLine("HelloBack: {0}", returnMessage));
            ret.Start();
            return ret;
        }
    }
}