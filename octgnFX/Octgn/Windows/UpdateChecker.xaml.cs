using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Xml;
using Octgn.Data;
using Octgn.Definitions;
using Octgn.Scripting;
using Skylabs.Lobby.Threading;
using vbAccelerator.Components.Shell;

namespace Octgn.Windows
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.Library;

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

        public UpdateChecker()
        {
            this.Loaded += OnLoaded;
            IsClosingDown = false;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_hasLoaded) return;
            _hasLoaded = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
#if(!DEBUG)
                CheckForUpdates();
#endif
                //CheckForXmlSetUpdates();
                this.LoadDatabase();
                UpdateCheckDone();

            });
            lblStatus.Content = "";
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
            this.UpdateStatus("Loaded database.");
        }

        private void CheckForUpdates()
        {
            UpdateStatus("Checking for updates...");
            try
            {
                string[] update = ReadUpdateXml(Program.UpdateInfoPath);


                Assembly assembly = Assembly.GetExecutingAssembly();
                Version local = assembly.GetName().Version;
                var online = new Version(update[0]);
                _isNotUpToDate = online > local;
                _updateURL = update[1];
                _downloadURL = update[2];
            }
            catch (Exception)
            {
                _isNotUpToDate = false;
                _downloadURL = "";
            }
        }

        private void UpdateCheckDone()
        {
            Dispatcher.Invoke(new Action(() =>
                                         {
                                             _realCloseWindow = true;
											 if (_isNotUpToDate)
											 {
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
													 if (!args.Cancelled)
													 {

														 LazyAsync.Invoke(()=> Process.Start(Path.Combine(Directory.GetCurrentDirectory(),filename)));
													 }
													 else
													 {
														 UpdateStatus("Downloading the new version failed. Please manually download.");
														 Process.Start(_downloadURL);
													 }
													 Close();
												 };
												 c.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs args)
												 {
													 progressBar1.Value = args.ProgressPercentage;
												 };
												 c.DownloadFileAsync(downloadUri, Path.Combine(Directory.GetCurrentDirectory(),filename));
											 }
											 else Close();
            }));
        }

        public void UpdateStatus(string stat)
        {
            Dispatcher.BeginInvoke(new Action(() =>
                                                  {
                                                      try
                                                      {
                                                          lblStatus.Content = stat;
                                                          listBox1.Items.Add(String.Format("[{0}] {1}" ,
                                                                                           DateTime.Now.
                                                                                               ToShortTimeString() ,
                                                                                           stat));
                                                          listBox1.SelectedIndex = listBox1.Items.Count - 1;
                                                      }
                                                      catch (Exception e)
                                                      {
                                                          if(Debugger.IsAttached)Debugger.Break();
                                                      }
                                                  }));
        }

        private static string[] ReadUpdateXml(string url)
        {
            var values = new string[3];
            try
            {
                WebRequest wr = WebRequest.Create(url);
                wr.Timeout = 15000;
                WebResponse resp = wr.GetResponse();
                Stream rgrp = resp.GetResponseStream();
                if (rgrp != null)
                {
                    using (XmlReader reader = XmlReader.Create(rgrp))
                    {
                        
                        while (reader.Read())
                        {
                            if (!reader.IsStartElement()) continue;
                            if (reader.IsEmptyElement) continue;
                            switch (reader.Name.ToLowerInvariant())
                            {
                                case "version":
                                    if (reader.Read())
                                        values[0] = reader.Value;
                                    break;
                                case "updatepath":
                                    //if (reader.Read())
                                        //values[1] = Program.WebsitePath + reader.Value;
                                    break;
                                case "installpath":
                                    if (reader.Read())
                                    {
                                        values[2] = Program.WebsitePath + reader.Value;
                                        values[1] = Program.WebsitePath + reader.Value;
                                    }
                                    break;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
#if(DEBUG)
                if (Debugger.IsAttached) Debugger.Break();
#endif
            }
            return values;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_realCloseWindow) e.Cancel = true;
        }
    }
}