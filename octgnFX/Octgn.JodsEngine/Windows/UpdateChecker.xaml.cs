using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

using Octgn.Annotations;
using Octgn.Library;

using log4net;

namespace Octgn.Windows
{
    public partial class UpdateChecker : INotifyPropertyChanged, ILoadingView
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool Shutdown { get; private set; }

        private bool _realCloseWindow = false;

        private readonly List<Func<ILoadingView, Task>> _loaders = new List<Func<ILoadingView, Task>>();

        public UpdateChecker() {
            Loaded += OnWindowLoaded;
            InitializeComponent();
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs routedEventArgs) {
            Loaded -= OnWindowLoaded;

            Log.Debug(nameof(OnWindowLoaded));

            try {
                UpdateStatus("Loading...");
                Log.Info("Loading...");

                await Task.Delay(60000);

                foreach (var loader in _loaders) {
                    await loader(this);
                }
            } catch (Exception ex) {
                Log.Error($"Error loading: {ex.Message}", ex);

                var message = $"There was an error loading Octgn. Please try again. If this continues to happen, let us know.";
                if (X.Instance.Debug) {
                    message = message + Environment.NewLine + ex.ToString();
                }

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                _realCloseWindow = true;

                Environment.ExitCode = 69;
                Shutdown = true;
            }

            Log.Info("Load complete");

            _realCloseWindow = true;

            // Expected: Managed Debugging Assistant NotMarshalable
            // See Also: http://stackoverflow.com/questions/31362077/loadfromcontext-occured
            Close();
        }

        public void AddLoader(Func<ILoadingView, Task> load) {
            _loaders.Add(load);
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

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ILoadingView
    {
        void AddLoader(Func<ILoadingView, Task> loader);
        void UpdateStatus(string status);
    }
}