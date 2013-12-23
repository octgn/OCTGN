using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Octgn.Core.Util;
using Skylabs.Lobby.Threading;

namespace Octgn.Windows
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Octgn.Annotations;
    using Octgn.Controls;
    using Octgn.Core;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.Library;
    using Octgn.Library.ExtensionMethods;

    using log4net;

    /// <summary>
    ///   Interaction logic for UpdateChecker.xaml
    /// </summary>
    public partial class UpdateChecker : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsClosingDown { get; set; }

        private bool _realCloseWindow = false;
        private bool _isNotUpToDate = false;

        private bool _hasLoaded = false;

        private Key[] keys = new Key[] { Key.None, Key.None, Key.None, Key.None, Key.None };
        private Key[] correctKeys = new Key[] { Key.O, Key.C, Key.T, Key.G, Key.N };

        private bool cancel = false;

        public string AdSource { get; set; }

        public UpdateChecker()
        {
            this.Loaded += OnLoaded;
            IsClosingDown = false;
            this.SetAdSource();
            InitializeComponent();
            this.PreviewKeyUp += OnPreviewKeyUp;
        }

        private void SetAdSource()
        {
            var r = new Random();
            var num = r.Next(0, 2);
            AdSource = "../Resources/LoadingWindowAds/" + num + ".jpg";
            this.OnPropertyChanged("AdSource");
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            bool gotOne = false;
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i] == Key.None)
                {
                    keys[i] = keyEventArgs.Key;
                    gotOne = true;
                    break;
                }
            }
            if (!gotOne)
            {
                Array.Copy(keys.ToArray(), 1, keys, 0, 4);
                keys[4] = keyEventArgs.Key;
            }
            if (keys.SequenceEqual(correctKeys))
            {
                // Blam
                cancel = true;
                //this.UpdateCheckDone();
            }
            //var sb = new StringBuilder();
            //foreach (var k in keys)
            //{
            //    sb.Append(new KeyConverter().ConvertTo(k, typeof(string)));
            //}
            //this.Title = sb.ToString();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Log.Info("Starting");
            if (_hasLoaded) return;
            _hasLoaded = true;
            var doingTable = false;
            try
            {
                if (Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().Contains("table"))) doingTable = true;
            }
            catch (Exception)
            {

            }
            ThreadPool.QueueUserWorkItem(s =>
            {
                UpdateStatus("Checking For Update");
                var updateDetails = UpdateManager.Instance.LatestVersion;
                if (updateDetails.CanUpdate)
                {
                    Dispatcher.Invoke(new Action(() => DownloadUpdate(updateDetails)));
                    return;
                }
                //#if(!DEBUG)
                if (doingTable == false)
                {
                    this.RandomMessage();
                    for (var i = 0; i < 20; i++)
                    {
                        Thread.Sleep(500);
                        if (cancel) break;
                    }
                    if (cancel)
                    {
                        this.UpdateCheckDone();
                        return;
                    }
                    this.ClearGarbage();
                    //CheckForXmlSetUpdates();
                }
                this.LoadDatabase();
                this.UpdateGames(doingTable);
                GameFeedManager.Get().OnUpdateMessage -= GrOnUpdateMessage;
                UpdateCheckDone();

            });
            lblStatus.Text = "";
            Log.Info("Finished");
        }

        private void RandomMessage()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var objStream = assembly.GetManifestResourceStream("Octgn.Resources.StartupMessages.txt");
            var objReader = new StreamReader(objStream);
            var lines = new List<string>();
            while (!objReader.EndOfStream)
            {
                lines.Add(objReader.ReadLine());
            }
            var rand = new Random();
            var linenum = rand.Next(0, lines.Count - 1);
            this.UpdateStatus(lines[linenum]);
        }

        private void LoadDatabase()
        {
            this.UpdateStatus("Loading games...");
            foreach (var g in GameManager.Get().Games)
            {
                Log.DebugFormat("Loaded Game {0}", g.Name);
            }
            this.UpdateStatus("Loading sets...");
            foreach (var s in SetManager.Get().Sets)
            {
                Log.DebugFormat("Loaded Set {0}", s.Name);
            }
            this.UpdateStatus("Loading scripts...");
            foreach (var s in DbContext.Get().Scripts)
            {
                Log.DebugFormat("Loading Script {0}", s.Path);
            }
            this.UpdateStatus("Loading proxies...");
            foreach (var p in DbContext.Get().ProxyDefinitions)
            {
                Log.DebugFormat("Loading Proxy {0}", p.Key);
            }
            this.UpdateStatus("Loaded database.");

            this.UpdateStatus("Migrating Images...");
            try
            {
                foreach (var g in GameManager.Get().Games)
                {
                    this.UpdateStatus(String.Format("Migrating {0} Images...", g.Name));
                    foreach (var s in g.Sets())
                    {
                        var gravePath = Paths.Get().GraveyardPath;
                        if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                        var dir = new DirectoryInfo(s.PackUri);
                        var newDir = new DirectoryInfo(s.ImagePackUri);
                        foreach (var f in dir.GetFiles("*.*"))
                        {
                            var newLocation = Path.Combine(newDir.FullName, f.Name);
                            f.MegaCopyTo(newLocation);
                            f.MoveTo(Path.Combine(gravePath, f.Name));
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Log.Warn("Migrate Files error", e);
                TopMostMessageBox.Show(
                    "There was an error migrating your image files. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            this.UpdateStatus("Migrated Images");

            this.UpdateStatus("Clearing Old Proxies...");
            try
            {
                foreach (var g in GameManager.Get().Games)
                {
                    this.UpdateStatus(String.Format("Clearing {0} Proxies...", g.Name));
                    foreach (var s in g.Sets())
                    {
                        var dir = new DirectoryInfo(s.ProxyPackUri);
                        if (dir.Exists)
                        {
                            var gravePath = Paths.Get().GraveyardPath;
                            if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                            foreach (var f in dir.GetFiles("*.*"))
                            {
                                f.MoveTo(Path.Combine(gravePath, f.Name));
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Log.Warn("Clearing Old Proxies error", e);
                TopMostMessageBox.Show(
                    "There was an error clearing your old proxies. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            this.UpdateStatus("Cleared Old Proxies");

            this.UpdateStatus("Clearing Old Installers...");
            try
            {
                var rpath = new DirectoryInfo(Paths.Get().BasePath);
                var gravePath = Paths.Get().GraveyardPath;
                if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                foreach (var f in rpath.GetFiles("OCTGN-Setup-*.exe"))
                {
                    if (f.Name.Contains(Const.OctgnVersion.ToString())) continue;
                    f.MoveTo(Path.Combine(gravePath,f.Name));
                }
            }
            catch (Exception e)
            {
                Log.Warn("Clearing Old Installers error", e);
                TopMostMessageBox.Show(
                    "There was an error clearing old installers. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            this.UpdateStatus("Cleared Old Installers");

        }

        private void ClearGarbage()
        {
            this.UpdateStatus("Clearing out garbage...");
            var gp = new DirectoryInfo(Paths.Get().GraveyardPath).Parent;
            foreach (var file in gp.GetFiles("*.*", SearchOption.AllDirectories))
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    Log.Warn("Couldn't delete garbage file " + file.FullName, e);
                }
            }
            for (var i = 0; i < 10; i++)
            {
                foreach (var dir in gp.GetDirectories("*", SearchOption.AllDirectories).ToArray())
                {
                    try
                    {
                        dir.Delete(true);

                    }
                    catch (Exception e)
                    {
                        Log.Warn("Couldn't delete garbage folder " + dir.FullName, e);
                    }
                }
            }
        }

        private void UpdateGames(bool localOnly)
        {
            this.UpdateStatus("Updating Games...This can take a little bit if there is an update.");
            var gr = GameFeedManager.Get();
            gr.OnUpdateMessage += GrOnUpdateMessage;
            Dispatcher.Invoke(new Action(() => { this.progressBar1.IsIndeterminate = false ; }));
            GameFeedManager.Get().CheckForUpdates(localOnly,
                (cur, max) => this.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            this.progressBar1.Maximum = max;
                            this.progressBar1.Value = cur;
                        })));
            Dispatcher.Invoke(new Action(() => { this.progressBar1.IsIndeterminate = true; }));
        }

        private void GrOnUpdateMessage(string s)
        {
            UpdateStatus(s);
        }

        private void DownloadUpdate(UpdateDetails details)
        {
            _realCloseWindow = true;
            Log.Info("Not up to date.");
            IsClosingDown = true;

            var downloadUri = new Uri(details.InstallUrl);
            string filename = System.IO.Path.GetFileName(downloadUri.LocalPath);

            if (details.UpdateDownloaded)
            {
                UpdateStatus("Launching Updater");
                Log.Info("Launching updater");
                Close();
                return;
            }

            UpdateStatus("Downloading new version.");

            var fd = new FileDownloader(downloadUri, Path.Combine(Directory.GetCurrentDirectory(), filename));

            progressBar1.Maximum = 100;
            progressBar1.IsIndeterminate = false;
            progressBar1.Value = 0;

            var myBinding = new Binding("Progress");
            myBinding.Source = fd;
            progressBar1.SetBinding(ProgressBar.ValueProperty, myBinding);

            var downloadTask = fd.Download();
            downloadTask.ContinueWith((t) =>
            {
                Log.Info("Download Complete");

                if (fd.DownloadFailed || !fd.DownloadComplete)
                {
                    Log.Info("Download Failed");
                    UpdateStatus("Downloading the new version failed. Please manually download.");
                    Program.LaunchUrl(details.InstallUrl);
                }
                else
                {
                    Log.Info("Launching updater");
                    UpdateStatus("Launching Updater");
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    progressBar1.IsIndeterminate = true;
                    Close();
                }));
            });
            downloadTask.Start();
        }

        private void UpdateCheckDone()
        {
            Log.Info("UpdateCheckDone");
            Dispatcher.Invoke(new Action(() =>
                                         {
                                             _realCloseWindow = true;
                                             Log.Info("Up to date...Closing");
                                             Close();
                                         }));
            Log.Info("UpdateCheckDone Complete");
        }

        public void UpdateStatus(string stat)
        {
            Log.Info(stat);
            Dispatcher.Invoke(new Action(() =>
                                                  {
                                                      try
                                                      {
                                                          lblStatus.Text = stat;
                                                          var str = String.Format("[{0}] {1}",DateTime.Now.ToShortTimeString(),stat);
                                                          LogItems.Children.Add(new ListBoxItem() { Content = str });
                                                          LogItemsScroller.ScrollToBottom();
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          Log.Error("Update status error", e);
                                                          if (Debugger.IsAttached) Debugger.Break();
                                                      }
                                                  }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Info("Closing Window");
            if (!_realCloseWindow)
            {
                Log.Info("Not a real close");
                e.Cancel = true;
            }
            else
                Log.Info("Real close");
        }

        private void ProgressBarMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                keys = correctKeys;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}