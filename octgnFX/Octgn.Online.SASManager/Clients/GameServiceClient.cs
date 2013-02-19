namespace Octgn.Online.SASManagerService.Clients
{
    using System;
    using System.Configuration;
    using System.Reflection;

    using Microsoft.AspNet.SignalR.Client;
    using Microsoft.AspNet.SignalR.Client.Hubs;

    using Octgn.Online.Library.SignalR.TypeSafe;
    using Octgn.Online.SASManagerService.Coms;

    using log4net;

    public class GameServiceClient : HubConnection, IDisposable
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static GameServiceClient current;
        private static readonly object Locker = new object();

        internal bool Stopped;

        public static GameServiceClient GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new GameServiceClient());
            }
        }
        #endregion

        internal IHubProxy HubProxy { get; set; }
        
        internal GameServiceClient()
            : base(ConfigurationManager.AppSettings["GameServiceHost"], false)
        {
            Log.Info("Creating");
            HubProxy = this.CreateHubProxy("SasManagerHub", new GameServiceToSASManagerService());
            this.StateChanged += this.ConnectionOnStateChanged;
            this.Reconnecting += this.ConnectionOnReconnecting;
            this.Reconnected += this.ConnectionOnReconnected;
            this.Received += this.ConnectionOnReceived;
            this.Error += this.ConnectionOnError;
            this.Closed += this.ConnectionOnClosed;
            Log.Info("Created");
        }

        public new void Start()
        {
            Log.Info("Starting");
            Stopped = false;
            base.Start();
            Log.Info("Started");
        }

        public new void Stop()
        {
            Log.Info("Stopping");
            Stopped = true;
            base.Stop();
            Log.Info("Stopped");
        }

        internal void Connect()
        {
            if (Stopped) return;
            base.Start().Wait(5000);
        }

        #region Connection Events

        protected void ConnectionOnStateChanged(StateChange stateChange)
        {
            Log.InfoFormat("State Changed: {0}->{1}",stateChange.OldState,stateChange.NewState);
        }

        protected void ConnectionOnReconnecting()
        {
            Log.Info("Reconnecting");
        }

        protected void ConnectionOnReconnected()
        {
            Log.Info("Reconnected");
        }

        protected void ConnectionOnReceived(string s)
        {
            //Log.InfoFormat("Received: {0}",s);
            
        }

        protected void ConnectionOnError(Exception exception)
        {
#if(!DEBUG)
            Log.Error("Connection Error",exception);
#endif
        }

        protected void ConnectionOnClosed()
        {
            Log.InfoFormat("Closed");
            if (!Stopped) this.Connect();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Stop();
            this.StateChanged -= this.ConnectionOnStateChanged;
            this.Reconnecting -= this.ConnectionOnReconnecting;
            this.Reconnected -= this.ConnectionOnReconnected;
            this.Received -= this.ConnectionOnReceived;
            this.Error -= this.ConnectionOnError;
            this.Closed -= this.ConnectionOnClosed;
        }
        #endregion
    }
}
