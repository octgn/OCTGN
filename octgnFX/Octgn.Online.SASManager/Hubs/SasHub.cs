namespace Octgn.Online.SASManagerService.Hubs
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.SignalR.Coms;

    using log4net;

    public class SasHub : Hub, ISASToSASManagerService
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

        public void HostedGameStateChanged(Guid id, EnumHostedGameStatus status)
        {
            HostedGameEngine.GetById(id).SetStatus(status);
            Log.InfoFormat("Game State Changed: {0} {1}",id,status);
        }
    }
}