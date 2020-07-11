﻿using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml;
using log4net;
using Octgn.Core.Util;
using Octgn.Library;
using System.Threading.Tasks;

namespace Octgn
{
    public class UpdateManager
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static UpdateManager SingletonContext { get; set; }

        private static readonly object UpdateManagerSingletonLocker = new object();

        public static UpdateManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (UpdateManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new UpdateManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public event EventHandler UpdateAvailable;

        private UpdateDetails LatestDetails { get; set; }

        private Timer Timer { get; set; }

        internal UpdateManager()
        {
            LatestDetails = new UpdateDetails(Const.OctgnVersion);
            //var a = Timeout.Infinite;
            Timer = new Timer(Tick, null, TimeSpan.FromMilliseconds(Timeout.Infinite), TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        public void Start()
        {
            Log.Info("Waiting to Start");
            lock (Timer)
            {
                Log.Info("Starting");
                Timer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(5));
            }
        }

        public void Stop()
        {
            Log.Info("Waiting to stop");
            lock (Timer)
            {
                Log.Info("Stopping");
                Timer.Change(TimeSpan.FromMilliseconds(Timeout.Infinite), TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        private void Tick(object state)
        {
            lock (Timer)
            {
                LatestDetails.UpdateInfo();
                if (LatestDetails.CanUpdate)
                {
                    DownloadLatestVersion();
                    if (LatestDetails.UpdateDownloaded)
                    {
                        FireOnUpdateAvailable();
                    }
                }
            }
        }

        public UpdateDetails LatestVersion
        {
            get
            {
                lock (LatestDetails)
                {
                    if (DateTime.Now > LatestDetails.LastCheckTime.AddMinutes(5) || LatestDetails.IsFaulted)
                    {
                        LatestDetails.UpdateInfo();
                    }
                    return LatestDetails;
                }
            }
        }

        public void DownloadLatestVersion()
        {
            lock (LatestDetails)
            {
                if (LatestDetails.CanUpdate && !LatestDetails.UpdateDownloaded)
                {
                    var downloadUri = new Uri(LatestDetails.InstallUrl);
                    string filename = System.IO.Path.GetFileName(downloadUri.LocalPath);
                    var filePath = Path.Combine(Config.Instance.Paths.UpdatesPath, filename);
                    var fd = new FileDownloader(downloadUri, filePath);
                    var dtask = fd.Download();
                    dtask.Start();
                    dtask.Wait();

                }
            }
        }

        public bool UpdateAndRestart()
        {
            lock (LatestDetails)
            {
                if (LatestDetails.CanUpdate && LatestDetails.UpdateDownloaded)
                {
                    var downloadUri = new Uri(LatestDetails.InstallUrl);
                    var filename = System.IO.Path.GetFileName(downloadUri.LocalPath);
                    var fi = new FileInfo(Path.Combine(Config.Instance.Paths.UpdatesPath, filename));
                    Task.Run(() => Program.LaunchApplication(fi.FullName));
                    Program.Exit();
                    return true;
                }
            }
            return false;
        }

        protected virtual void FireOnUpdateAvailable()
        {
            EventHandler handler = UpdateAvailable;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

    public class UpdateDetails
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Version Version { get; set; }
        public string InstallUrl { get; set; }
        public DateTime LastCheckTime { get; set; }
        public bool IsFaulted { get; set; }

        public bool UpdateDownloaded
        {
            get
            {
                if (!CanUpdate) return false;
                var downloadUri = new Uri(InstallUrl);
                var filename = System.IO.Path.GetFileName(downloadUri.LocalPath);
                var fi = new FileInfo(Path.Combine(Config.Instance.Paths.UpdatesPath, filename));
                if (fi.Exists)
                {
                    var remoteLength = new FileDownloader(downloadUri, filename).GetRemoteFileSize();
                    for (var i = 0; i < 3; i++)
                    {
                        if (remoteLength != -1) break;
                        remoteLength = new FileDownloader(downloadUri, filename).GetRemoteFileSize();
                        Thread.Sleep(1000);
                    }

                    if (remoteLength == -1) return false;

                    if (fi.Length >= remoteLength)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private readonly Version _currentVerison;

        public UpdateDetails(Version currentVersion)
        {
            _currentVerison = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            IsFaulted = true;
            LastCheckTime = DateTime.MinValue;
        }

        public bool? IsUpToDate
        {
            get
            {
                if (Version == null)
                    return null;
                var thisVersion = GetType().Assembly.GetName().Version;

                if (Version.Minor != thisVersion.Minor) return true;

                return Version.Equals(thisVersion);
            }
        }

        public bool CanUpdate
        {
            get
            {
                var iu = IsUpToDate ?? true;
                if (IsFaulted) return false;
                return iu == false;
            }
        }

        public UpdateDetails UpdateInfo()
        {
            lock (this)
            {
                Version = null;
                InstallUrl = null;
                this.LastCheckTime = DateTime.Now;
                IsFaulted = true;
                var url = AppConfig.UpdateInfoPath;
                try
                {
                    var c = new Octgn.Site.Api.ApiClient();
                    var info = c.GetLatestRelease(_currentVerison);
                    if (Program.IsReleaseTest == false)
                    {
                        Version = Version.Parse(info.LiveVersion);
                        this.InstallUrl = info.LiveVersionDownloadLocation;
                    }
                    else
                    {
                        Version = Version.Parse(info.TestVersion);
                        this.InstallUrl = info.TestVersionDownloadLocation;
                    }
                    if (!String.IsNullOrWhiteSpace(InstallUrl) && Version != null)
                    {
                        IsFaulted = false;
                    }
                }
                catch (WebException e)
                {
                    Log.Warn("", e);
                    IsFaulted = true;
                }
                catch (Exception e)
                {
                    Log.Warn("", e);
                    IsFaulted = true;
                }
                return this;
            }
        }
    }
}