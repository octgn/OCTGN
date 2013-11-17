using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Octgn.Library;

namespace Octgn.Core.Util
{
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
            Log.InfoFormat("Created filedownloader for url {0} and filename {1}", Url,Filename);
        }

        public FileDownloader(Uri url, DirectoryInfo folder)
        {
            Url = url;
            string filename = new FileInfo(System.IO.Path.GetFileName(url.LocalPath)).Name;
            Filename = Path.Combine(folder.FullName, filename);
            Client = new WebClient();
            Client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            Client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
            Log.InfoFormat("Created filedownloader for url {0} and filename {1}", Url,Filename);
        }

        public Task Download()
        {
            return new Task(DoDownload);
        }

        public long GetRemoteFileSize()
        {
            try
            {
                using (var obj = new WebClient()) using (var s = obj.OpenRead(Url)) return long.Parse(obj.ResponseHeaders["Content-Length"].ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Log.Warn("Error GetRemoteFileSize", e);
            }
            return -1;
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