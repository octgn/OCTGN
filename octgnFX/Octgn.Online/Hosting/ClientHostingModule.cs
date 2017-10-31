using Octgn.Communication;
using Octgn.Communication.Serializers;
using System;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public class ClientHostingModule : IClientModule
    {
        public IClientHostingRPC RPC { get; set; }

        public event EventHandler<HostedGameReadyEventArgs> HostedGameReady;

        public ClientHostingModule(Client client, Version octgnVersion) {
            RPC = new ClientHostingRPC(client, octgnVersion);

            if(client.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }
        }

        public Task HandleRequest(object sender, HandleRequestEventArgs args) {
            return Task.CompletedTask;
        }
    }
}
