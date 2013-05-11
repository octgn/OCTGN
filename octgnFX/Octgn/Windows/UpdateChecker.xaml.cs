using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml;

using Skylabs.Lobby.Threading;

namespace Octgn.Windows
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.Library;
    using Octgn.Library.ExtensionMethods;

    using log4net;

    /// <summary>
    ///   Interaction logic for UpdateChecker.xaml
    /// </summary>
    public partial class UpdateChecker
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsClosingDown { get; set; }

        private bool _realCloseWindow = false;
        private bool _isNotUpToDate = false;
        private string _downloadURL = "";
        private string _updateURL = "";

        private bool _hasLoaded = false;

        private Key[] keys = new Key[] { Key.None, Key.None, Key.None, Key.None, Key.None, Key.None, };
        private Key[] correctKeys = new Key[]{Key.N,Key.U,Key.D, Key.I, Key.T, Key.Y};

        private bool cancel = false;

        public UpdateChecker()
        {
            this.Loaded += OnLoaded;
            IsClosingDown = false;
            InitializeComponent();
            this.PreviewKeyUp += OnPreviewKeyUp;
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            bool gotOne = false;
            for(var i =0;i<keys.Length;i++)
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
                Array.Copy(keys.ToArray(),1,keys,0,5);
                keys[5] = keyEventArgs.Key;
            }
            if (keys.SequenceEqual(correctKeys))
            {
                // Blam
                cancel = true;
                //this.UpdateCheckDone();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Log.Info("Starting");
            if (_hasLoaded) return;
            _hasLoaded = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
#if(!DEBUG)
                if (CheckForUpdates())
                {
                    Dispatcher.Invoke(new Action(Update));
                    return;
                }
#endif
                this.RandomMessage();
                for (var i = 0; i < 10; i++)
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
                this.LoadDatabase();
                this.UpdateGames();
                UpdateCheckDone();

            });
            lblStatus.Text = "";
            Log.Info("Finsihed");
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
                Log.DebugFormat("Loaded Game {0}",g.Name);
            }
            this.UpdateStatus("Loading sets...");
            foreach (var s in SetManager.Get().Sets)
            {
                Log.DebugFormat("Loaded Set {0}",s.Name);
            }
            this.UpdateStatus("Loading scripts...");
            foreach (var s in DbContext.Get().Scripts)
            {
                Log.DebugFormat("Loading Script {0}",s.Path);
            }
            this.UpdateStatus("Loading proxies...");
            foreach (var p in DbContext.Get().ProxyDefinitions)
            {
                Log.DebugFormat("Loading Proxy {0}",p.Key);
            }
            this.UpdateStatus("Loaded database.");
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
                catch(Exception e)
                {
                    Log.Warn("Couldn't delete garbage file " + file.FullName,e);
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
                    catch(Exception e)
                    {
                        Log.Warn("Couldn't delete garbage folder " + dir.FullName, e);
                    }
                }
            }
        }

        private void UpdateGames()
        {
            this.UpdateStatus("Updating Games...This can take a little bit if there is an update.");
            Task.Factory.StartNew(GameFeedManager.Get().CheckForUpdates).Wait(TimeSpan.FromMinutes(5));
        }

        private void Update()
        {
            _realCloseWindow = true;
            Log.Info("Not up to date.");
            IsClosingDown = true;

            var downloadUri = new Uri(_updateURL);
            string filename = System.IO.Path.GetFileName(downloadUri.LocalPath);

            UpdateStatus("Downloading new version.");
            var c = new WebClient();
            progressBar1.Maximum = 100;
            progressBar1.IsIndeterminate = false;
            progressBar1.Value = 0;
            c.DownloadFileCompleted += delegate(object sender, AsyncCompletedEventArgs args)
            {
                Log.Info("Download complete");
                if (!args.Cancelled)
                {
                    Log.Info("Launching updater");
                    LazyAsync.Invoke(
                        () =>Program.LaunchUrl(Path.Combine(Directory.GetCurrentDirectory(), filename)));
                }
                else
                {
                    Log.Info("Download failed");
                    UpdateStatus("Downloading the new version failed. Please manually download.");
                    Program.LaunchUrl(_downloadURL);
                }
                Close();
            };
            c.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs args)
            { progressBar1.Value = args.ProgressPercentage; };
            Log.InfoFormat("Downloading new version to {0}", filename);
            c.DownloadFileAsync(downloadUri, Path.Combine(Directory.GetCurrentDirectory(), filename));
        }

        private bool CheckForUpdates()
        {
            UpdateStatus("Checking for updates...");
            try
            {
                Log.InfoFormat("Getting update info from {0}",AppConfig.UpdateInfoPath);
                string[] update = ReadUpdateXml(AppConfig.UpdateInfoPath);
                Log.Info("Got update info");
                
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version local = assembly.GetName().Version;
                var online = new Version(update[0]);
                _isNotUpToDate = online > local;
                Log.InfoFormat("Online: {0} Local:{1}",online,local);
                _updateURL = update[1];
                _downloadURL = update[2];
                if (_isNotUpToDate) return true;
            }
            catch (Exception e)
            {
                _isNotUpToDate = false;
                _downloadURL = "";
                Log.Warn("Check For Updates Error",e);
            }
            return false;
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
            Dispatcher.BeginInvoke(new Action(() =>
                                                  {
                                                      try
                                                      {
                                                          lblStatus.Text = stat;
                                                          listBox1.Items.Add(String.Format("[{0}] {1}" ,
                                                                                           DateTime.Now.
                                                                                               ToShortTimeString() ,
                                                                                           stat));
                                                          listBox1.SelectedIndex = listBox1.Items.Count - 1;
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          Log.Error("Update status error",e);
                                                          if(Debugger.IsAttached)Debugger.Break();
                                                      }
                                                  }));
        }

        private static string[] ReadUpdateXml(string url)
        {
            Log.Info("Reading update xml");
            var values = new string[3];
            try
            {
                Log.InfoFormat("Downloading info from {0}", url);
                WebRequest wr = WebRequest.Create(url);
                wr.Timeout = 15000;
                WebResponse resp = wr.GetResponse();
                Stream rgrp = resp.GetResponseStream();
                Log.Info("Got stream");
                if (rgrp != null)
                {
                    Log.Info("Creating reader");
                    using (XmlReader reader = XmlReader.Create(rgrp))
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
                                        values[0] = reader.Value;
                                    }
                                    break;
                                case "updatepath":
                                    //if (reader.Read())
                                    //values[1] = Program.WebsitePath + reader.Value;
                                    break;
                                case "installpath":
                                    if (reader.Read())
                                    {
                                        Log.InfoFormat("Reading paths {0} {1}", reader.Value, reader.Value);
                                        values[2] = AppConfig.WebsitePath + reader.Value;
                                        values[1] = AppConfig.WebsitePath + reader.Value;
                                    }
                                    break;

                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                Log.Warn("",e);
            }
            catch (Exception e)
            {
                Log.Error("Error",e);
                Debug.WriteLine(e);
#if(DEBUG)
                if (Debugger.IsAttached) Debugger.Break();
#endif
            }
            return values;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Info("Closing Window");
            if (!_realCloseWindow)
            {
                Log.Info("Not a real close");
                e.Cancel = true;
            }else
            Log.Info("Real close");
        }
    }
}