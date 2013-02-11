namespace Octgn.Online.SASManagerService.Clients
{
    using System;
    using System.Configuration;
    using System.Reflection;

    using Microsoft.AspNet.SignalR.Client;

    using Octgn.Online.Library.SignalR;

    using log4net;

    public class GameServiceClient : Client
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static GameServiceClient current;
        private static readonly object Locker = new object();
        public static GameServiceClient GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new GameServiceClient());
            }
        }
        #endregion
        internal GameServiceClient()
        {
            Log.Info("Creating");
            this.Setup(ConfigurationManager.AppSettings["GameServiceHost"], "SasManagerHub");
            Log.Info("Created");
        }

        public new void Start()
        {
            Log.Info("Starting");
            base.Start();
            Log.Info("Started");
        }

        public new void Stop()
        {
            Log.Info("Stopping");
            base.Stop();
            Log.Info("Stopped");
        }

        #region Connection Events

        protected override void ConnectionOnStateChanged(StateChange stateChange)
        {
            Log.InfoFormat("State Changed: {0}->{1}",stateChange.OldState,stateChange.NewState);
        }

        protected override void ConnectionOnReconnecting()
        {
            Log.Info("Reconnecting");
        }

        protected override void ConnectionOnReconnected()
        {
            Log.Info("Reconnected");
        }

        protected override void ConnectionOnReceived(string s)
        {
            Log.InfoFormat("Received: {0}",s);
            
        }

        protected override void ConnectionOnError(Exception exception)
        {
#if(!DEBUG)
            Log.Error("Connection Error",exception);
#endif
        }

        protected override void ConnectionOnClosed()
        {
            Log.InfoFormat("Closed");
        }
        #endregion
    }
}
