namespace Octgn.Online.GameService.Hubs
{
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using Octgn.Online.GameService.Coms;

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
            var mess = new GameServiceToSASManagerService(this.Clients.Caller);
            mess.Hello("hello1", "Hello2");
            return mess.Return;
            //return this.Clients.Caller.FunActions(1,"hi",new ObfuscateAssemblyAttribute(false),new SasManagerHub());
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
