using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using Octgn.Annotations;
using Octgn.Controls;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.DataManagers;
using Octgn.DataNew;
using Octgn.Library;
using Octgn.Library.ExtensionMethods;

using log4net;

namespace Octgn.Windows
{
    public partial class UpdateChecker : INotifyPropertyChanged
    {
        internal new static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _realCloseWindow = false;

        private bool _hasWindowLoaded = false;

        private Key[] keys = new Key[] { Key.None, Key.None, Key.None, Key.None, Key.None };
        private Key[] correctKeys = new Key[] { Key.O, Key.C, Key.T, Key.G, Key.N };

        private bool cancel = false;

        public string AdSource { get; set; }

        public UpdateChecker() {
            Loaded += OnWindowLoaded;
            SetAdSource();
            InitializeComponent();
            PreviewKeyUp += OnPreviewKeyUp;
        }

        private void SetAdSource() {
            var r = new Random();
            var num = r.Next(0, 2);
            num = 0;
            AdSource = "../Resources/LoadingWindowAds/" + num + ".jpg";
            OnPropertyChanged("AdSource");
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            bool gotOne = false;
            for (var i = 0; i < keys.Length; i++) {
                if (keys[i] == Key.None) {
                    keys[i] = keyEventArgs.Key;
                    gotOne = true;
                    break;
                }
            }
            if (!gotOne) {
                Array.Copy(keys.ToArray(), 1, keys, 0, 4);
                keys[4] = keyEventArgs.Key;
            }
            if (keys.SequenceEqual(correctKeys)) {
                // Blam
                cancel = true;
            }
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs routedEventArgs) {
            if (_hasWindowLoaded) return;
            _hasWindowLoaded = true;

            Log.Debug(nameof(OnWindowLoaded));

            try {
                await Task.Run(LoadOctgn);
            } catch (Exception ex) {
                Log.Error($"Error loading: {ex.Message}", ex);

                MessageBox.Show($"There was an error loading Octgn. Please try again. If this continues to happen, let us know.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                _realCloseWindow = true;

                ((OctgnApp)(OctgnApp.Current)).Shutdown(69);

                Program.Exit();

                Close();
            }
        }

        public void AddLoader(Action load) {
            _loaders.Add(load);
        }

        private readonly List<Action> _loaders = new List<Action>();

        private void LoadOctgn() {
            UpdateStatus("Loading...");
            Log.Info("Loading...");

            var doingTable = false;
            try {
                if (Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().Contains("table"))) doingTable = true;
            } catch (Exception ex) {
                Log.Error($"Error reading env vars for table: ${ex.Message}", ex);
            }

            if (doingTable == false) {
                RandomMessage();

                for (var i = 0; i < 20; i++) {
                    Thread.Sleep(250);

                    if (cancel) break;
                }

                ClearGarbage();
            }

            if (cancel) {
                Log.Info("Skipped loading database");
            } else {
                LoadDatabase();
            }

            foreach (var loader in _loaders) {
                loader();
            }

            Log.Info("Load complete");

            _realCloseWindow = true;

            // Expected: Managed Debugging Assistant NotMarshalable
            // See Also: http://stackoverflow.com/questions/31362077/loadfromcontext-occured
            Close();

        }

        private void RandomMessage() {
            var assembly = Assembly.GetExecutingAssembly();
            var objStream = assembly.GetManifestResourceStream("Octgn.Resources.StartupMessages.txt");
            var objReader = new StreamReader(objStream);
            var lines = new List<string>();
            while (!objReader.EndOfStream) {
                lines.Add(objReader.ReadLine());
            }
            var rand = new Random();
            var linenum = rand.Next(0, lines.Count - 1);
            UpdateStatus(lines[linenum]);
        }

        private void LoadDatabase() {
            UpdateStatus("Loading games...");
            foreach (var g in GameManager.Get().Games) {
                Log.DebugFormat("Loaded Game {0}", g.Name);
            }
            UpdateStatus("Loading sets...");
            foreach (var s in SetManager.Get().Sets) {
                Log.DebugFormat("Loaded Set {0}", s.Name);
            }
            UpdateStatus("Loading scripts...");
            foreach (var s in DbContext.Get().Scripts) {
                Log.DebugFormat("Loading Script {0}", s.Path);
            }
            UpdateStatus("Loading proxies...");
            foreach (var p in DbContext.Get().ProxyDefinitions) {
                Log.DebugFormat("Loading Proxy {0}", p.Key);
            }
            UpdateStatus("Loaded database.");

            UpdateStatus("Migrating Images...");
            try {
                foreach (var g in GameManager.Get().Games) {
                    UpdateStatus(String.Format("Migrating {0} Images...", g.Name));
                    foreach (var s in g.Sets()) {
                        var gravePath = Config.Instance.Paths.GraveyardPath;
                        if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                        var dir = new DirectoryInfo(s.PackUri);
                        var newDir = new DirectoryInfo(s.ImagePackUri);
                        foreach (var f in dir.GetFiles("*.*")) {
                            var newLocation = Path.Combine(newDir.FullName, f.Name);
                            f.MegaCopyTo(newLocation);
                            f.MoveTo(Path.Combine(gravePath, f.Name));
                        }
                    }

                }
            } catch (Exception e) {
                Log.Warn("Migrate Files error", e);
                TopMostMessageBox.Show(
                    "There was an error migrating your image files. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            UpdateStatus("Migrated Images");

            UpdateStatus("Clearing Old Proxies...");
            try {
                foreach (var g in GameManager.Get().Games) {
                    UpdateStatus(String.Format("Clearing {0} Proxies...", g.Name));
                    foreach (var s in g.Sets()) {
                        var dir = new DirectoryInfo(s.ProxyPackUri);
                        if (dir.Exists) {
                            var gravePath = Config.Instance.Paths.GraveyardPath;
                            if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                            foreach (var f in dir.GetFiles("*.*")) {
                                f.MoveTo(Path.Combine(gravePath, f.Name));
                            }
                        }
                    }

                }
            } catch (Exception e) {
                Log.Warn("Clearing Old Proxies error", e);
                TopMostMessageBox.Show(
                    "There was an error clearing your old proxies. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            UpdateStatus("Cleared Old Proxies");

            UpdateStatus("Clearing Old Installers...");
            try {
                var rpath = new DirectoryInfo(Config.Instance.Paths.BasePath);
                var gravePath = Config.Instance.Paths.GraveyardPath;
                if (!Directory.Exists(gravePath)) Directory.CreateDirectory(gravePath);
                foreach (var f in rpath.GetFiles("OCTGN-Setup-*.exe")) {
                    if (f.Name.Contains(Const.OctgnVersion.ToString())) continue;
                    f.MoveTo(Path.Combine(gravePath, f.Name));
                }
            } catch (Exception e) {
                Log.Warn("Clearing Old Installers error", e);
                TopMostMessageBox.Show(
                    "There was an error clearing old installers. Restarting your computer may help",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            UpdateStatus("Cleared Old Installers");

        }

        private void ClearGarbage() {
            UpdateStatus("Clearing out garbage...");
            try {
                var gp = new DirectoryInfo(Config.Instance.Paths.GraveyardPath).Parent;
                foreach (var file in gp.GetFiles("*.*", SearchOption.AllDirectories)) {
                    try {
                        file.Delete();
                    } catch (Exception e) {
                        Log.Warn("Couldn't delete garbage file " + file.FullName, e);
                    }
                }
                for (var i = 0; i < 10; i++) {
                    foreach (var dir in gp.GetDirectories("*", SearchOption.AllDirectories).ToArray()) {
                        try {
                            dir.Delete(true);

                        } catch (Exception e) {
                            Log.Warn("Couldn't delete garbage folder " + dir.FullName, e);
                        }
                    }
                }

            } catch (Exception e) {
                Log.Info("Clear Garbage Error", e);
            }
        }

        public void UpdateStatus(string stat) {
            Log.Info(stat);
            Dispatcher.Invoke(new Action(() => {
                try {
                    lblStatus.Text = stat;
                    var str = String.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), stat);
                    LogItems.Children.Add(new ListBoxItem() { Content = str });
                    LogItemsScroller.ScrollToBottom();
                } catch (Exception e) {
                    Log.Error("Update status error", e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Log.Info("Closing Window");
            if (!_realCloseWindow) {
                Log.Info("Not a real close");
                e.Cancel = true;
            } else
                Log.Info("Real close");
        }

        private void ProgressBarMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                keys = correctKeys;
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}