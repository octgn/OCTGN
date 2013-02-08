namespace Octgn.Online.GameService.Hubs
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    public class SasManagerHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }
}
