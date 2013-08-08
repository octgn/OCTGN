using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using Octgn.Library;

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

        private UpdateDetails LatestDetails { get; set; }

        internal UpdateManager()
        {
            LatestDetails = new UpdateDetails();
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
                    
                }
            }
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
                var fi = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), filename));
                if (fi.Exists)
                {
                    if (fi.Length >= new FileDownloader(downloadUri, filename).GetRemoteFileSize())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public UpdateDetails()
        {
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
                if (Version > thisVersion)
                {
                    return false;
                }
                return true;
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
                Log.Info("Reading update xml");
                var url = AppConfig.UpdateInfoPath;
                try
                {
                    Log.InfoFormat("Downloading info from {0}", url);
                    var wr = WebRequest.Create(url);
                    wr.Timeout = 15000;
                    var resp = wr.GetResponse();
                    var rgrp = resp.GetResponseStream();
                    Log.Info("Got stream");
                    if (rgrp != null)
                    {
                        Log.Info("Creating reader");
                        using (var reader = XmlReader.Create(rgrp))
                        {
                            Log.Info("Created reader...reading");

                            while (reader.Read())
                            {
                                if (!reader.IsStartElement()) continue;
                                if (reader.IsEmptyElement) continue;
                                switch (reader.Name.ToLowerInvariant())
                                {
                                    case "version":
                                        if (reader.Read())
                                        {
                                            Log.InfoFormat("Reading version {0}", reader.Value);
                                            Version = Version.Parse(reader.Value);
                                        }
                                        break;
                                    case "installpath":
                                        if (reader.Read())
                                        {
                                            Log.InfoFormat("Reading paths {0} {1}", reader.Value, reader.Value);
#if(Release_Test)
                                            InstallUrl = "https://s3.amazonaws.com/octgn/releases/test/" + reader.Value.Replace("downloadtest/", "");
#else
                                            InstallUrl = "https://s3.amazonaws.com/octgn/releases/live/" + reader.Value.Replace("download/", "");
#endif
                                        }
                                        break;
                               }
                            }
                        }
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

    public class FileDownloader : IDisposable, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int _progress;
        private bool _isDownloading;
        private bool _downloadFailed;
        private bool _downloadComplete;

        internal Uri Url { get; set; }
        internal string Filename { get; set; }
        internal WebClient Client { get; set; }

        public int Progress
        {
            get { return _progress; }
            set
            {
                if (value == _progress) return;
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                if (value == _isDownloading) return;
                _isDownloading = value;
                OnPropertyChanged("IsDownloading");
            }
        }

        public bool DownloadFailed
        {
            get { return _downloadFailed; }
            set
            {
                if (value == _downloadFailed) return;
                _downloadFailed = value;
                OnPropertyChanged("DownloadFailed");
            }
        }

        public bool DownloadComplete
        {
            get { return _downloadComplete; }
            set
            {
                if (value == _downloadComplete) return;
                _downloadComplete = value;
                OnPropertyChanged("DownloadComplete");
            }
        }

        public FileDownloader(Uri url, string filename)
        {
            Url = url;
            Filename = filename;
            Client = new WebClient();
            Client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            Client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
        }

        public Task Download()
        {
            return new Task(DoDownload);
        }

        public long GetRemoteFileSize()
        {
            using (var obj = new WebClient())
            using (var s = obj.OpenRead(Url))
                return long.Parse(obj.ResponseHeaders["Content-Length"].ToString(CultureInfo.InvariantCulture));
        }

        private void DoDownload()
        {
            Log.Info("Waiting on DoDownload");
            lock (this)
            {
                Log.Info("Doing Download");
                if (IsDownloading)
                {
                    Log.Warn("Already DoDownloading");
                    return;
                }
                try
                {
                    IsDownloading = true;
                    DownloadFailed = false;
                    DownloadComplete = false;
                    Progress = 0;
                    if (File.Exists(Filename))
                    {
                        Log.Warn("File " + Filename + " Already Exists...Moving to graveyard");
                        var gr = Paths.Get().GraveyardPath;
                        if (!Directory.Exists(gr))
                            Directory.CreateDirectory(gr);
                        var grp = Path.Combine(gr, new FileInfo(Filename).Name);
                        File.Move(Filename, grp);
                    }
                    Log.InfoFormat("Downloading {0} to {1}",Url,Filename);
                    Client.DownloadFileAsync(Url, Filename);
                    while (DownloadComplete == false)
                    {
                        Thread.Sleep(0);
                    }
                    if(DownloadFailed)
                        throw new Exception("Download Failed");
                    Log.InfoFormat("Finished downloading {0} to {1} successfully",Url,Filename);
                }
                catch (Exception e)
                {
                    Log.Warn("Couldn't download " + Url + " to " + Filename,e);
                    DownloadFailed = true;
                    IsDownloading = false;
                }
            }
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            Progress = args.ProgressPercentage;
        }

        private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
        {
            if (args.Cancelled || args.Error != null)
            {
                this.DownloadFailed = true;
                if (args.Error != null)
                {
                    Log.Warn("FileDownloader Error",args.Error);
                }
            }
            DownloadComplete = true;
        }


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Client.DownloadProgressChanged -= ClientOnDownloadProgressChanged;
            Client.Dispose();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}