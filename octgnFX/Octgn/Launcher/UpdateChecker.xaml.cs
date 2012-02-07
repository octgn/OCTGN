using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Octgn.Definitions;
using System.IO;
using Skylabs.Lobby.Threading;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for UpdateChecker.xaml
    /// </summary>
    public partial class UpdateChecker : Window
    {
        public bool IsClosingDown { get; set; }
        private bool stopReading = false;
        private List<string> Errors = new List<string>();
        public UpdateChecker()
        {
            IsClosingDown = false;
            InitializeComponent();
            LazyAsync.Invoke(VerifyAllDefs);
            lblStatus.Content = "";
            //Thread t = new Thread(VerifyAllDefs);
            //t.Start();
        }
        private void CheckForUpdates()
        {
            try
            {
                bool isupdate = false;
                string ustring = "";
                string[] update = new string[2];
                update = ReadUpdateXML("http://www.skylabsonline.com/downloads/octgn/update.xml");


                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Version local = assembly.GetName().Version;
                Version online = new Version(update[0]);
                isupdate = online > local;
                ustring = update[1];
                Dispatcher.BeginInvoke(new Action<bool, string>(UpdateCheckDone), isupdate, ustring);
            }
            catch (Exception)
            {
                Dispatcher.BeginInvoke(new Action<bool, string>(UpdateCheckDone), false, "");
            }
        }
        private void UpdateCheckDone(bool result, string url)
        {
            if (result)
            {
                IsClosingDown = true;
                switch (
                    MessageBox.Show("An update is available. Would you like to download now?", "Update Available",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        System.Diagnostics.Process.Start(url);
                        break;
                }
            }
            this.Close();
        }
        private void VerifyAllDefs()
        {
            UpdateStatus("Loading Game Definitions...");
            try
            {
                if (Program.GamesRepository == null)
                    Program.GamesRepository = new Octgn.Data.GamesRepository();
                Octgn.Scripting.Engine engine;
                List<Data.Game> g2r = new List<Data.Game>();
                foreach (Octgn.Data.Game g in Program.GamesRepository.Games)
                {
                    UpdateStatus("Checking Game: " + g.Name);
                    if (!File.Exists(g.Filename))
                    {
                        Errors.Add("[" + g.Name + "]: Def file doesn't exist at " + g.Filename);
                        continue;
                    }
                    Program.Game = new Game(GameDef.FromO8G(g.Filename));
                    Program.Game.TestBegin();
                    IEnumerable<Play.Player> plz = Play.Player.All;
                    engine = new Scripting.Engine(true);
                    string[] terr = engine.TestScripts(Program.Game);
                    Program.Game.End();
                    if (terr.Length > 0)
                    {
                        Errors.AddRange(terr);
                        g2r.Add(g);

                    }
                }
                foreach (Octgn.Data.Game g in g2r)
                    Program.GamesRepository.Games.Remove(g);
                if (Errors.Count > 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        String ewe = "";
                        foreach (string s in Errors)
                            ewe += s + Environment.NewLine;
                        ErrorWindow er = new ErrorWindow(ewe);
                        er.ShowDialog();
                    }));

                }
                UpdateStatus("Checking for updates...");
            }
            catch (Exception)
            {
                //System.Diagnostics.Debugger.Break();
            }
            CheckForUpdates();
        }
        private void UpdateStatus(string stat)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    lblStatus.Content = stat;
                }
                catch (Exception)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }));
        }
        private bool FileExists(string URL)
        {
            bool result = false;
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                try
                {
                    System.IO.Stream str = client.OpenRead(URL);
                    if (str != null) result = true; else result = false;
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }
        private string[] ReadUpdateXML(string URL)
        {
            string[] values = new string[2];
            try
            {
                System.Net.WebRequest wr = System.Net.WebRequest.Create(URL);
                wr.Timeout = 15000;
                System.Net.WebResponse resp = wr.GetResponse();
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(resp.GetResponseStream()))
                {
                    while (reader.Read())
                    {
                        if (stopReading)
                            break;
                        if (reader.IsStartElement())
                        {
                            if (!reader.IsEmptyElement)
                            {
                                switch (reader.Name)
                                {
                                    case "Version":
                                        values = new string[2];
                                        if (reader.Read()) { values[0] = reader.Value; }
                                        break;
                                    case "Location":
                                        if (reader.Read()) { values[1] = reader.Value; }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            finally
            {

            }
            return values;
        }
    }
}
