using System;
using System.Collections.Generic;
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

namespace Octgn.Windows
{
    /// <summary>
    ///   Interaction logic for UpdateChecker.xaml
    /// </summary>
    public partial class UpdateChecker
    {
        public bool IsClosingDown { get; set; }

        private bool _realCloseWindow = false;
        private readonly List<string> _errors = new List<string>();
        private bool _isUpToDate = false;
        private string _downloadURL = "";

        public UpdateChecker()
        {
            IsClosingDown = false;
            InitializeComponent();
            if (Program.GamesRepository == null)
                Program.GamesRepository = new GamesRepository();
            ThreadPool.QueueUserWorkItem(s =>
            {
                if (Prefs.InstallOnBoot)
                {
                    InstallDefsFromFolders();
                    InstallSetsFromFolders();
                }
                VerifyAllDefs();
                CheckForUpdates();
                UpdateCheckDone(_isUpToDate,_downloadURL);
            });
            lblStatus.Content = "";
        }

        private void InstallDefsFromFolders()
        {
            UpdateStatus("Checking folders for games that aren't installed.");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            var path2 = GamesRepository.BasePath;

            //Grab all def files
            var defList = Directory.GetFiles(path , "*.o8g" , SearchOption.AllDirectories);
            String[] defList2 = new string[0];
            if(path != path2)
                defList2 = Directory.GetFiles(path2, "*.o8g", SearchOption.AllDirectories);

            //Install if they aren't already
            var dList = defList.Union(defList2);
            foreach(var d in dList)
            {
                var gd = GameDef.FromO8G(d);
                var og = Program.GamesRepository.AllGames.SingleOrDefault(x => x.Id == gd.Id);
                if(og != null)
                {
                    if (gd.Version > og.Version || (gd.Version == og.Version && gd.FileHash != og.FileHash))
                    {
                        UpdateStatus("Installing game " + gd.Name);
                        if(!gd.Install())
                            UpdateStatus("Couldn't install game " + gd.Name);
                        else
                            UpdateStatus("Installed game " + gd.Name);
                    }
                }
                else
                {
                    UpdateStatus("Installing game " + gd.Name);
                    if (!gd.Install())
                        UpdateStatus("Couldn't install game " + gd.Name);
                    else
                        UpdateStatus("Installed game " + gd.Name);
                }
            }
        }

        private void InstallSetsFromFolders()
        {
            UpdateStatus("Checking folders for sets that aren't installed.");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
            var path2 = GamesRepository.BasePath;

            //Grab all def files
            var setList = Directory.GetFiles(path, "*.o8s", SearchOption.AllDirectories);
            String[] setList2 = new string[0];
            if (path != path2)
                setList2 = Directory.GetFiles(path2, "*.o8s", SearchOption.AllDirectories);

            //Install if they aren't already
            var sList = setList.Union(setList2);
            foreach(var s in sList)
            {
                try
                {
                    var ns = Set.SetFromFile(s , Program.GamesRepository);
                    if (ns.Game == null) continue;
                    var osg = Program.GamesRepository.AllGames.SingleOrDefault(x => x.Id == ns.Game.Id);
                    if(osg == null)
                        continue;
                    var os = osg.GetSet(ns.Id);
                    if(os == null)
                        InstallSet(s,osg);
                    else if(ns.Version > os.Version)
                        InstallSet(s,osg);                    
                }
                catch(Exception e)
                {
                    UpdateStatus("Could not process set " + s + ": " + e.Message);
                    UpdateStatus("---------------------");
                    UpdateStatus(e.StackTrace);
                    UpdateStatus("---------------------");
                }


            }
            Prefs.InstallOnBoot = false;
        }

        private void InstallSet(string fname, Octgn.Data.Game SelectedGame)
        {
            string shortName = Path.GetFileName(fname);
            UpdateStatus("Installing Set " + shortName);
            string path = Path.Combine(Prefs.DataDirectory, "Games", SelectedGame.Id.ToString(), "Sets");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            
            try
            {
                if (shortName != null)
                {
                    string copyto = Path.Combine(path, shortName);
                    if (fname.ToLower() != copyto.ToLower())
                        File.Copy(fname, copyto, true);
                    SelectedGame.InstallSet(copyto);
                }
                UpdateStatus(string.Format("Set '{0}' installed.", shortName));
            }
            catch (Exception ex)
            {
                UpdateStatus(string.Format("'{0}' an error occured during installation:",shortName));
                UpdateStatus(ex.Message);
            }
        }

        private void CheckForUpdates()
        {
            UpdateStatus("Checking for updates...");
            try
            {
                string[] update = ReadUpdateXml("https://raw.github.com/kellyelton/Octgn/master/currentversion.xml");


                Assembly assembly = Assembly.GetExecutingAssembly();
                Version local = assembly.GetName().Version;
                var online = new Version(update[0]);
                _isUpToDate = online > local;
                _downloadURL = update[1];
            }
            catch (Exception)
            {
                _isUpToDate = false;
                _downloadURL = "";
            }
        }

        private void UpdateCheckDone(bool result, string url)
        {
            Dispatcher.Invoke(new Action(() =>
                                         {
                                             _realCloseWindow = true;
                if (result)
                {
                    IsClosingDown = true;
                    
                    switch (
                        MessageBox.Show("An update is available. Would you like to download now?", "Update Available",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            Process.Start(url);
                            break;
                    }
                }
                
                Close();
            }));
        }

        private void VerifyAllDefs()
        {
            UpdateStatus("Loading Game Definitions...");
            try
            {
                var g2R = new List<Data.Game>();
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    foreach (Data.Game g in Program.GamesRepository.Games)
                    {
                        string fhash = "";

                        UpdateStatus("Checking Game: " + g.Name);
                        var fpath = g.FullPath;
                        if (!File.Exists(fpath))
                        {
                            _errors.Add("[" + g.Name + "]: Def file doesn't exist at " + fpath);
                            continue;
                        }
                        using (var file = new FileStream(fpath, FileMode.Open))
                        {
                            byte[] retVal = md5.ComputeHash(file);
                            fhash = BitConverter.ToString(retVal).Replace("-", ""); // hex string
                        }
                        if (fhash.ToLower() == g.FileHash.ToLower()) continue;

                        Program.Game = new Game(GameDef.FromO8G(fpath));
                        Program.Game.TestBegin();
                        //IEnumerable<Player> plz = Player.All;
                        var engine = new Engine(true);
                        string[] terr = engine.TestScripts(Program.Game);
                        Program.Game.End();
                        if (terr.Length <= 0)
                        {
                            Program.GamesRepository.UpdateGameHash(g,fhash);
                            continue;
                        }
                        _errors.AddRange(terr);
                        g2R.Add(g);
                    }
                }
                foreach (Data.Game g in g2R)
                    Program.GamesRepository.Games.Remove(g);
                if (_errors.Count > 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                                                          {
                                                              String ewe = _errors.Aggregate("",
                                                                                             (current, s) =>
                                                                                             current +
                                                                                             (s + Environment.NewLine));
                                                              var er = new Windows.ErrorWindow(ewe);
                                                              er.ShowDialog();
                                                          }));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            
        }

        private void UpdateStatus(string stat)
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
                                                      catch (Exception)
                                                      {
                                                          Debugger.Break();
                                                      }
                                                  }));
        }

        private static string[] ReadUpdateXml(string url)
        {
            var values = new string[2];
            try
            {
                WebRequest wr = WebRequest.Create(url);
                wr.Timeout = 15000;
                WebResponse resp = wr.GetResponse();
                Stream rgrp = resp.GetResponseStream();
                if (rgrp != null)
                    using (XmlReader reader = XmlReader.Create(rgrp))
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsStartElement()) continue;
                            if (reader.IsEmptyElement) continue;
                            switch (reader.Name)
                            {
                                case "Version":
                                    values = new string[2];
                                    if (reader.Read())
                                    {
                                        values[0] = reader.Value;
                                    }
                                    break;
                                case "Location":
                                    if (reader.Read())
                                    {
                                        values[1] = reader.Value;
                                    }
                                    break;
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

        public static bool CheckGameDef(GameDef game)
        {
            Program.Game = new Game(game);
            Program.Game.TestBegin();
            var engine = new Engine(true);
            string[] terr = engine.TestScripts(Program.Game);
            Program.Game.End();
            if (terr.Length > 0)
            {
                String ewe = terr.Aggregate("",
                                            (current, s) =>
                                            current +
                                            (s + Environment.NewLine));
                var er = new Windows.ErrorWindow(ewe);
                er.ShowDialog();
            }
            return terr.Length == 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_realCloseWindow) e.Cancel = true;
        }
    }
}