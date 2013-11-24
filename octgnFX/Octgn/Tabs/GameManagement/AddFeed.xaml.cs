namespace Octgn.Tabs.GameManagement
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using UserControl = System.Windows.Controls.UserControl;

    public partial class AddFeed: UserControl,IDisposable
    {
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(sender, result);
            }
        }

        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(String), typeof(AddFeed));

        public bool HasErrors { get; private set; }
        public string Error
        {
            get { return this.GetValue(ErrorProperty) as String; }
            private set { this.SetValue(ErrorProperty, value); }
        }

        public static DependencyProperty FeedNameProperty = DependencyProperty.Register(
            "FeedName", typeof(String), typeof(AddFeed));

        public string FeedName
        {
            get { return this.GetValue(FeedNameProperty) as String; }
            private set { this.SetValue(FeedNameProperty, value); }
        }

        public static DependencyProperty FeedUrlProperty = DependencyProperty.Register(
            "FeedUrl", typeof(String), typeof(AddFeed));

        public string FeedUrl
        {
            get { return this.GetValue(FeedUrlProperty) as String; }
            private set { this.SetValue(FeedUrlProperty, value); }
        }

        private Decorator Placeholder;

        public AddFeed()
        {
            InitializeComponent();
        }

        void ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FeedName))
                this.SetError("You must enter a feed name");
            else if (String.IsNullOrWhiteSpace(FeedUrl))
                this.SetError("You must enter a feed path");
            else if (!Core.GameFeedManager.Get().ValidateFeedUrl(FeedUrl)) 
                this.SetError("This feed is not valid");
            else
                this.SetError();
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            this.Error = error;
        }

        #region Dialog
        public void Show(Decorator placeholder)
        {
            this.Placeholder = placeholder;

            placeholder.Child = this;
        }

        public void Close()
        {
            this.Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            ProgressBar.IsIndeterminate = false;
            this.Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            BorderHostGame.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
        }

        void EndWait()
        {
            BorderHostGame.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.IsIndeterminate = false;
        }

        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            if (FeedName == null) FeedName = "";
            if (FeedUrl == null) FeedUrl = "";
            FeedName = FeedName.Trim();
            FeedUrl = FeedUrl.Trim();
            this.ValidateFields();
            Program.Dispatcher = this.Dispatcher;
            if (this.HasErrors) return;
            this.StartWait();
            var feedName = FeedName;
            var feedUrl = FeedUrl;
            var task = new Task(() => Core.GameFeedManager.Get().AddFeed(feedName,feedUrl));
            task.ContinueWith((continueTask) =>
                {
                    var error = "";
                    if (continueTask.IsFaulted)
                    {
                        if (continueTask.Exception != null) error = continueTask.Exception.Message;
                        else error = "Unable to add feed. Try again later.";
                    }
                    this.Dispatcher.Invoke(new Action(() =>
                        {
                            if(!string.IsNullOrWhiteSpace(error))
                                this.SetError(error);
                            this.EndWait();
                            if (string.IsNullOrWhiteSpace(error))
                                this.Close(DialogResult.OK);
                        }));
                });
            task.Start();
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object, DialogResult>)d;
                }
            }
        }

        #endregion
    }
}
