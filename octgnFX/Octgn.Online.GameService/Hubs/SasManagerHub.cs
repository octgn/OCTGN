namespace Octgn.Online.GameService.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.SignalR.Coms;
    using Octgn.Online.Library.SignalR.TypeSafe;

    using log4net;

    public class SasManagerHub : Hub, ISASToSASManagerService
    {
        #region Static
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static ConcurrentStack<string> Connections { get; set; }

        /// <summary>
        /// Grab the next connectionId. Returns null if no connections exist
        /// </summary>
        public static string NextConnectionId {
            get
            {
                lock (Connections)
                {
                    var id = "";
                    if (Connections.Count == 0) return null;
                    while (!Connections.TryPop(out id))
                    {
                        if (Connections.Count == 0) return null;
                        Thread.Sleep(10);
                    }
                    Connections.Push(id);
                    return id;
                }
            }
        }

        /// <summary>
        /// Used to send messages out when not responding to an event.
        /// </summary>
        public static HubContextMessenger<IGameServiceToSASManagerService> Out
        {
            get
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<SasManagerHub>();
                return HubContextMessenger<IGameServiceToSASManagerService>.Get(hubContext.Clients);
            }
        }

        static SasManagerHub()
        {
            Connections = new ConcurrentStack<string>();
        }

        #endregion



        #region ConnectionEvents
        public override Task OnConnected()
        {
            Log.InfoFormat("Connected {0}", this.Context.ConnectionId);
            
            Connections.Push(this.Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            Log.InfoFormat("Disconnected {0}", this.Context.ConnectionId);
            var id = "";

            lock (Connections)
            {
                var arr = Connections.ToList();
                arr.Remove(this.Context.ConnectionId);
                Connections.Clear();
                Connections.PushRange(arr.ToArray());
            }
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            Log.InfoFormat("Reconnected {0}", this.Context.ConnectionId);
            Connections.Push(this.Context.ConnectionId);
            return base.OnReconnected();
        }
        #endregion

        public void HostedGameStateChanged(Guid id, EnumHostedGameStatus status)
        {
            Log.InfoFormat("Game State Changed: {0} {1}",id,status);
        }
    }
}
