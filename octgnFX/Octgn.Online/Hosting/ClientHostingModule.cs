using Octgn.Communication;
using System;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public class ClientHostingModule : IClientModule
    {
        public IClientHostingRPC RPC { get; set; }

        public event EventHandler<HostedGameReadyEventArgs> HostedGameReady;

        public Task HandleRequest(object sender, HandleRequestEventArgs args) {
            return Task.CompletedTask;
        }
    }
}
