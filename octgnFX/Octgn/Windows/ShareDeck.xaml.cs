namespace Octgn.Windows
{
    using System.ComponentModel;

    using Octgn.Annotations;

    /// <summary>
    /// Interaction logic for ShareDeck.xaml
    /// </summary>
    public partial class ShareDeck : INotifyPropertyChanged
    {
        private string errorText;

        private string shareUrl;

        private bool enableInput;

        private bool showProgressBar;

        public bool ShowError
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ErrorText);
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
            set
            {
                if (value == this.errorText)
                {
                    return;
                }
                this.errorText = value;
                this.OnPropertyChanged("ErrorText");
                this.OnPropertyChanged("ShowError");
            }
        }

        public bool ShowShareUrl
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ShareUrl);
            }
        }

        public string ShareUrl
        {
            get
            {
                return this.shareUrl;
            }
            set
            {
                if (value == this.shareUrl)
                {
                    return;
                }
                this.shareUrl = value;
                this.OnPropertyChanged("ShareUrl");
                this.OnPropertyChanged("ShowShareUrl");
            }
        }

        public bool EnableInput
        {
            get
            {
                return this.enableInput;
            }
            set
            {
                if (value.Equals(this.enableInput))
                {
                    return;
                }
                this.enableInput = value;
                this.OnPropertyChanged("EnableInput");
            }
        }

        public bool ShowProgressBar
        {
            get
            {
                return this.showProgressBar;
            }
            set
            {
                if (value.Equals(this.showProgressBar))
                {
                    return;
                }
                this.showProgressBar = value;
                this.OnPropertyChanged("ShowProgressBar");
            }
        }

        public ShareDeck()
        {
            EnableInput = true;
            InitializeComponent();
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
