namespace Octgn.Online.GameService
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Timers;

    using Sidewinder.Core;

    using log4net;

    public static class UpdateManager
    {
        public static event EventHandler OnUpdateDetected; 
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static Timer UpdateTimer;
        public static bool Update()
        {
            Log.Info("Checking for updates");
            var factory = AppUpdateFactory.Setup(DoUpdate);
            var ret = factory.Execute();
            Log.Info(ret?"Update available":"Update not available");
            return ret;
        }

        public static void Start()
        {
            Log.Info("Starting");
            UpdateTimer = new Timer(1000 * 60);
            UpdateTimer.Elapsed += UpdateTimerOnElapsed;
            UpdateTimer.Start();
            Log.Info("Started");
        }

        public static void Stop()
        {
            Log.Info("Stopping");
            UpdateTimer.Stop();
            UpdateTimer.Close();
            UpdateTimer.Dispose();
            Log.Info("Stopped");
        }

        internal static void FireOnUpdateDetected()
        {
            Log.Info("Firing OnUpdateDetected");
            if(OnUpdateDetected != null)
                OnUpdateDetected(new object(), new EventArgs());
        }

        internal static void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
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

        internal static void DoUpdate(UpdateConfigBuilder updateConfigBuilder)
        {
            updateConfigBuilder = updateConfigBuilder.Update(
                Assembly.GetExecutingAssembly().GetName().Name, ConfigurationManager.AppSettings["UpdateFeed"])
                               .OverwriteContentFiles();
            Log.InfoFormat("Current Version: {0}",updateConfigBuilder.CurrentAppVersion());
        }

    }
}
