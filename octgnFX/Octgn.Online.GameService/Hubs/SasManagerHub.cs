namespace Octgn.Online.GameService.Hubs
{
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using log4net;

    public class SasManagerHub : Hub
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public SasManagerHub()
        {
            
        }
        public override Task OnConnected()
        {
            Log.InfoFormat("Connected {0}", this.Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            Log.InfoFormat("Disconnected {0}", this.Context.ConnectionId);
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            Log.InfoFormat("Reconnected {0}", this.Context.ConnectionId);
            return base.OnReconnected();
        }
    }
}
