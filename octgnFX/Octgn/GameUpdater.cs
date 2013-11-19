namespace Octgn
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Timers;

    using Octgn.Core;
    using Octgn.Library;

    using log4net;

    public class GameUpdater:IDisposable
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
            //Log.Info("Creating");
            //LocalFeedWatcher = new FileSystemWatcher(Paths.Get().LocalFeedPath);
            //LocalFeedWatcher.Changed += FileWatcherEvent;
            //LocalFeedWatcher.Created += FileWatcherEvent;
            //LocalFeedWatcher.Deleted += FileWatcherEvent;
            //LocalFeedWatcher.Renamed += FileWatcherEvent;
            //Log.Info("Created");
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal const int RefreshTime = 10 * 60 * 1000; //Believe that's 10 minutes
        public bool IsRunning { get; internal set; }
        internal Timer RefreshTimer { get; set; }
        internal FileSystemWatcher LocalFeedWatcher { get; set; }

        internal virtual void FileWatcherEvent(object sender, FileSystemEventArgs args)
        {
            try
            {
                Log.InfoFormat("File event received {0} {1}",args.FullPath,args.ChangeType);
                this.RefreshTimerOnElapsed(this, null);
                Log.InfoFormat("File event no errors {0}", args.FullPath);
            }
            finally
            {
                Log.InfoFormat("File event complete {0}", args.FullPath);
            }
        }

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
        internal static readonly object timerLock = new object();
        internal virtual void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Info("Updater ticks");
            lock (timerLock)
            {
                Log.Info("Entered timer lock");
                if (WindowManager.PlayWindow != null || WindowManager.DeckEditor != null)
                {
                    Log.InfoFormat(
                        "Can't update PlayWindow={0} DeckEditor={1}",
                        WindowManager.PlayWindow != null,
                        WindowManager.DeckEditor != null);
                    return;
                }
                Log.Info("Checking for updates");
                GameFeedManager.Get().CheckForUpdates(true);
                Log.InfoFormat("Update check complete");
            }
            Log.Info("Exited timer lock");
        }
        #endregion Timer

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (LocalFeedWatcher != null)
            {
                LocalFeedWatcher.Changed -= FileWatcherEvent;
                LocalFeedWatcher.Created -= FileWatcherEvent;
                LocalFeedWatcher.Deleted -= FileWatcherEvent;
                LocalFeedWatcher.Renamed -= FileWatcherEvent;
            }
            DestroyTimer();
        }

        #endregion
    }
}