namespace Octgn.Online.StandAloneServer.Hubs
{
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using log4net;

    public class ClientHub: Hub
    {
        #region Static
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region ConnectionEvents
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
        #endregion
    }
}