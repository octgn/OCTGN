namespace Octgn.Online.Library.UpdateManager
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Timers;

    using Sidewinder.Core;

    using log4net;

    public class UpdateManager
    {
        #region Context
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static UpdateManager Context;
        private static readonly object Locker = new object();
        public static UpdateManager GetContext()
        {
            lock(Locker)
                return Context ?? (Context = new UpdateManager());
        }

        #endregion

        public event EventHandler OnUpdateDetected; 
        internal Timer UpdateTimer;
        internal UpdateManagerConfig Config;
        internal UpdateManager()
        {
            Log.Info("Loading config values");

            var ass = Assembly.GetEntryAssembly();
            var execonfig = ConfigurationManager.OpenExeConfiguration(ass.Location);
            var confObj = execonfig.GetSection("UpdateManagerConfig");
            Config = (execonfig.GetSection("UpdateManagerConfig") as UpdateManagerConfig)
                         ?? new UpdateManagerConfig();
            Log.Info("Setting Update Frequency: " + Config.UpdateFrequency);
            Log.Info("Setting Update Feed: " + Config.UpdateFeed);
        }
        public bool Update()
        {
            Log.Info("Checking for updates");
            if (!Config.Enabled)
            {
                Log.Info("Update Manager Disabled via config file.");
                return false;
            }
            var factory = AppUpdateFactory.Setup(DoUpdate);
            var ret = factory.Execute();
            Log.Info(ret?"Update available":"Update not available");
            return ret;
        }

        public void Start()
        {
            Log.Info("Starting");
            if (!Config.Enabled)
            {
                Log.Info("Update Manager Disabled via config file.");
                return;
            }
            UpdateTimer = new Timer(Config.UpdateFrequency);
            UpdateTimer.Elapsed += UpdateTimerOnElapsed;
            UpdateTimer.Start();
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            if (!Config.Enabled)
            {
                Log.Info("Update Manager Disabled via config file.");
                return;
            }
            UpdateTimer.Stop();
            UpdateTimer.Close();
            UpdateTimer.Dispose();
            Log.Info("Stopped");
        }

        internal void FireOnUpdateDetected()
        {
            Log.Info("Firing OnUpdateDetected");
            if(OnUpdateDetected != null)
                OnUpdateDetected(new object(), new EventArgs());
        }

        internal void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Info("Checking for update...");
            if (Update())
            {
                Log.Info("Update Found");
                FireOnUpdateDetected();
                return;
            }
            Log.Info("No Update Found");
        }

        internal void DoUpdate(UpdateConfigBuilder updateConfigBuilder)
        {
            updateConfigBuilder = updateConfigBuilder.Update(
                Config.PackageName, Config.UpdateFeed)
                               .OverwriteContentFiles();
            Log.InfoFormat("Current Version: {0}",updateConfigBuilder.CurrentAppVersion());
        }

    }
}
