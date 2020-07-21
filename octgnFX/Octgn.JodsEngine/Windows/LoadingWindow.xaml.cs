using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

using Octgn.Annotations;
using Octgn.Library;

using log4net;
using Octgn.Loaders;
using Octgn.Launchers;
using Octgn.Library.Exceptions;
using System.Threading.Tasks;

namespace Octgn.Windows
{
    public partial class LoadingWindow : INotifyPropertyChanged, ILoadingView
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _realCloseWindow = false;

        public LoadingWindow() {
            Loaded += OnWindowLoaded;

            InitializeComponent();
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs routedEventArgs) {
            Loaded -= OnWindowLoaded;

            Log.Debug(nameof(OnWindowLoaded));

            var shutdown = false;
            try {
                UpdateStatus("Loading...");
                Log.Info("Loading...");

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);

                var loader = new Loader();

                loader.Loaders.Add(new ConfigLoader());
                loader.Loaders.Add(new NetworkLoader());
                loader.Loaders.Add(new GraphicsLoader());
                loader.Loaders.Add(new GameMessageLoader());
                loader.Loaders.Add(new EnvironmentLoader());
                loader.Loaders.Add(new VersionedLoader());

                await loader.Load(this);

                UpdateStatus("Launching...");

                await Task.Delay(100);

                if (Program.Launcher == null) {
                    Log.Warn($"No launcher specified, using Deck Editor");
                    Program.Launcher = new DeckEditorLauncher();
                }

                if (!await Program.Launcher.Launch(this)) {
                    shutdown = true;
                }
            } catch (UserMessageException ex) {
                Log.Error($"Error loading: {ex.Message}", ex);

                var message = ex.Message;
                if (X.Instance.Debug) {
                    message = message + Environment.NewLine + ex.ToString();
                }

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.ExitCode = 70;

                shutdown = true;
            } catch (Exception ex) {
                Log.Error($"Error loading: {ex.Message}", ex);

                var message = $"There was an error loading Octgn. Please try again. If this continues to happen, let us know.";
                if (X.Instance.Debug) {
                    message = message + Environment.NewLine + ex.ToString();
                }

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.ExitCode = 69;

                shutdown = true;
            }

            _realCloseWindow = true;

            if (shutdown) {
                Log.Warn("Shutting down");

                Program.Exit();
            } else {
                Log.Info("Load complete");
            }

            // Expected: Managed Debugging Assistant NotMarshalable
            // See Also: http://stackoverflow.com/questions/31362077/loadfromcontext-occured
            Close();
        }

        public void UpdateStatus(string stat) {
            Log.Info(stat);

            _ = Dispatcher.InvokeAsync(new Action(() => {
                lblStatus.Text = stat;
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            Log.Info("Closing Window");
            if (!_realCloseWindow) {
                Log.Info("Not a real close");
                e.Cancel = true;
            } else
                Log.Info("Real close");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ILoadingView
    {
        void UpdateStatus(string status);
    }
}