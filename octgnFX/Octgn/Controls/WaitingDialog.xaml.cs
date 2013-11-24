namespace Octgn.Controls
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Forms;

    public partial class WaitingDialog : INotifyPropertyChanged ,IDisposable
    {
        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                if (value == this.message) return;
                this.message = value;
                OnPropertyChanged("Message");
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (value == this.title) return;
                this.title = value;
                OnPropertyChanged("Title");
            }
        }

        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(sender, result);
            }
        }

        private Decorator Placeholder;

        private string message;

        private string title;

        public WaitingDialog()
        {
            InitializeComponent();
        }

        #region Dialog
        public void Show(Decorator placeholder, Action action, string argtitle, string argmessage)
        {
            Placeholder = placeholder;
            placeholder.Child = this;
            Title = argtitle;
            Message = argmessage;
            var task = new Task(action);
            task.ContinueWith((x) => this.Close());
            task.Start();
        }

        public void UpdateMessage(string message)
        {
            Dispatcher.Invoke(new Action(() =>
                { this.Message = message; }));
        }

        public void Close()
        {
            Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ProgressBar.IsIndeterminate = false;
                                              this.Placeholder.Child = null;
                                              this.FireOnClose(this, result);
            }));
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
            if (PropertyChanged != null)
            {
                foreach (var d in PropertyChanged.GetInvocationList())
                {
                    PropertyChanged -= (PropertyChangedEventHandler)d;
                }
            }
        }

        #endregion
    }
}
