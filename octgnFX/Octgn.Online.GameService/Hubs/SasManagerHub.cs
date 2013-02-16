namespace Octgn.Online.GameService.Hubs
{
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using Octgn.Online.GameService.Coms;
    using Octgn.Online.Library.SignalR;
    using Octgn.Online.Library.SignalR.Coms;

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

            this.Send<IGameServiceToSASManagerService>().All.Hello("hello1","hello2");

            HubMessenger<IGameServiceToSASManagerService>.Get(this.Clients)
                .All.Hello("hello1", "hello2");
            //var mess = new GameServiceToSASManagerService(this.Clients.Caller);
            //mess.Hello("hello1", "Hello2");
            return new Task(() => { });
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
