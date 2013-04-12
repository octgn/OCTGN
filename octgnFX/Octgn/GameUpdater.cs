namespace Octgn
{
    using System.Reflection;
    using System.Timers;

    using Octgn.Core;

    using log4net;

    public class GameUpdater
    {
        #region Singleton

        internal static GameUpdater SingletonContext { get; set; }

        private static readonly object GameUpdaterSingletonLocker = new object();

        public static GameUpdater Get()
        {
            lock (GameUpdaterSingletonLocker) return SingletonContext ?? (SingletonContext = new GameUpdater());
        }

        internal GameUpdater()
        {
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal const int RefreshTime = 10 * 60 * 1000; //Believe that's 10 minutes
        public bool IsRunning { get; internal set; }
        internal Timer RefreshTimer { get; set; }

        #region StartStop
        public void Start()
        {
            Log.Info("Try Start");
            if (IsRunning) return;
            Log.Info("Start");
            IsRunning = true;
            ConstructTimer();
            Log.Info("Start Finished");
        }

        public void Stop()
        {
            Log.Info("Try Stop");
            if (!IsRunning) return;
            Log.Info("Stop");
            IsRunning = false;
            this.DestroyTimer();
            Log.Info("Stop Finished");
        }
        #endregion StartStop
        #region Timer
        internal virtual void ConstructTimer()
        {
            Log.Info("Constructing Timer");
            this.DestroyTimer();
            RefreshTimer = new Timer(RefreshTime);
            RefreshTimer.Elapsed += RefreshTimerOnElapsed;
            RefreshTimer.Start();
            this.RefreshTimerOnElapsed(null, null);
            Log.Info("Constructing Timer Complete");
        }

        internal virtual void DestroyTimer()
        {
            Log.Info("Destroying Timer");
            if (RefreshTimer != null)
            {
                try { RefreshTimer.Elapsed -= this.RefreshTimerOnElapsed; }
                catch { }
                try { RefreshTimer.Stop(); }
                catch { }
                try { RefreshTimer.Dispose(); }
                catch { }
                RefreshTimer = null;
            }
            Log.Info("Timer destruction complete");
        }
        internal virtual void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Info("Timer Ticks");
            if (WindowManager.PlayWindow != null || WindowManager.DeckEditor != null) return;
            GameFeedManager.Get().CheckForUpdates();
        }
        #endregion Timer
    }
}