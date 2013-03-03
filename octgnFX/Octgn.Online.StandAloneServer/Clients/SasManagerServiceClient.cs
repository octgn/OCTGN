namespace Octgn.Online.StandAloneServer.Clients
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using KellyElton.SignalR.TypeSafe.ExtensionMethods;

    using Microsoft.AspNet.SignalR.Client;
    using Microsoft.AspNet.SignalR.Client.Hubs;

    using Octgn.Online.Library.Coms;
    using Octgn.Online.Library.Enums;
    using Octgn.Online.StandAloneServer.Coms;

    using log4net;

    using ConnectionState = Microsoft.AspNet.SignalR.Client.ConnectionState;

    public class SasManagerServiceClient : HubConnection, IDisposable
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static SasManagerServiceClient current;
        private static readonly object Locker = new object();

        public ConnectionState ConnectionState { get; internal set; }

        internal bool Stopped;

        public static SasManagerServiceClient GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new SasManagerServiceClient());
            }
        }
        #endregion

        internal IHubProxy HubProxy { get; set; }

        internal SasManagerServiceClient()
            : base(ConfigurationManager.AppSettings["SasManagerServiceHost"], false)
        {
            Log.Info("Creating");
            HubProxy = this.CreateHubProxy("SasHub", new SasManagerServiceToSas());
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
            try
            {
                GameStateEngine.GetContext().SetStatus(EnumHostedGameStatus.GameShuttingDown);
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
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
            switch (stateChange.NewState)
            {
                case ConnectionState.Connecting:
                    break;
                case ConnectionState.Connected:
                    // Created a task because if reconnecting, it seems that this message doesn't go through if it doesn't have a delay
                    var t = new Task(() =>
                        {
                            Thread.Sleep(1000);
                            if(GameStateEngine.GetContext().State.Status == EnumHostedGameStatus.Booting)
                                GameStateEngine.GetContext().SetStatus(EnumHostedGameStatus.Booted);
                            else
                                GameStateEngine.GetContext().SetStatus(GameStateEngine.GetContext().Game.Status);
                        });
                    t.Start();
                    break;
                case ConnectionState.Reconnecting:
                    break;
                case ConnectionState.Disconnected:
                    break;
            }
            ConnectionState = stateChange.NewState;
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