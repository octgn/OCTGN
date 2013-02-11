namespace Octgn.Online.SASManagerService.Clients
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;

    using Microsoft.AspNet.SignalR.Client;
    using Microsoft.AspNet.SignalR.Client.Hubs;

    using log4net;

    public class GameServiceClient
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
        internal HubConnection Connection;
        internal bool Stopped;
        internal bool Started;
        internal GameServiceClient()
        {
            Log.Info("Creating");
            Connection = new HubConnection(ConfigurationManager.AppSettings["GameServiceHost"],false);
            Connection.CreateHubProxy("SasManagerHub");
            Connection.Closed += ConnectionOnClosed;
            Connection.Error += ConnectionOnError;
            Connection.Received += ConnectionOnReceived;
            Connection.Reconnected += ConnectionOnReconnected;
            Connection.Reconnecting += ConnectionOnReconnecting;
            Connection.StateChanged += ConnectionOnStateChanged;
            Log.Info("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            if(Started)
                throw new InvalidOperationException("GameServiceClient already started.");
            Started = true;
            Stopped = false;
            this.Connect();
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            Stopped = true;
            Started = false;
            Connection.Stop();
            Log.Info("Stopped");
        }

        internal void Connect()
        {
            var connected = false;
            while (!connected)
            {
                Log.Info("Connecting");
                if (Stopped)
                {
                    Log.Info("Stopping connecting: Stop called.");
                    break;
                }

                Connection.Start().ContinueWith(task =>
                    {
                        if (!task.IsFaulted && !task.IsCanceled)
                        {
                            Log.Info("Connected");
                            connected = true;
                        }
                        else
                        {
                            var ex = new Exception();
                            if (task.Exception != null)
                                ex = task.Exception.InnerExceptions.FirstOrDefault() ?? ex;
                            Log.Warn("Connection failed...retrying",ex);
                        }
                    }).Wait(2000);
            }
        }

        #region Connection Events
        private void ConnectionOnStateChanged(StateChange stateChange)
        {
            Log.InfoFormat("State Changed: {0}->{1}",stateChange.OldState,stateChange.NewState);
        }

        private void ConnectionOnReconnecting()
        {
            Log.Info("Reconnecting");
        }

        private void ConnectionOnReconnected()
        {
            Log.Info("Reconnected");
        }

        private void ConnectionOnReceived(string s)
        {
            Log.InfoFormat("Received: {0}",s);
            
        }

        private void ConnectionOnError(Exception exception)
        {
#if(!DEBUG)
            Log.Error("Connection Error",exception);
#endif
        }

        private void ConnectionOnClosed()
        {
            Log.InfoFormat("Closed");
            if(Stopped == false)
                this.Connect();
        }
        #endregion
    }
}
