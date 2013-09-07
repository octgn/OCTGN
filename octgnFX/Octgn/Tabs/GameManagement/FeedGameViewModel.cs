namespace Octgn.Tabs.GameManagement
{
    using System;
    using System.ComponentModel;

    using NuGet;

    using Octgn.Annotations;
    using Octgn.Core.DataManagers;

    public class FeedGameViewModel : INotifyPropertyChanged,IEquatable<FeedGameViewModel>,IDisposable
    {
        private IPackage package;
        private Guid id;
        public IPackage Package
        {
            get
            {
                return this.package;
            }
            set
            {
                //if (Equals(value, this.package))
                //{
                //    return;
                //}
                this.package = value;
                if (!Guid.TryParse(package.Id, out this.id)) this.id = Guid.Empty;
                this.OnPropertyChanged("Package");
                this.OnPropertyChanged("Installed");
                this.OnPropertyChanged("ImageUri");
                this.OnPropertyChanged("Id");
                this.OnPropertyChanged("InstallButtonText");
            }
        }
        public Guid Id{get{return id;}}
        public bool Installed
        {
            get
            {
                var isInstalled = GameManager.Get().GetById(Id) != null;
                return isInstalled;
            }
        }
        public Version InstalledVersion
        {
            get
            {
                if(!Installed)return new Version();
                return GameManager.Get().GetById(Id).Version;
            }
        }
        public string InstallButtonText
        {
            get
            {
                return Installed ? "Uninstall" : "Install";
            }
        }
        public Uri ImageUri
        {
            get
            {
                return Package == null
                           ? new Uri("pack://application:,,,/Octgn;Component/Resources/FileIcons/Game.ico")
                           : Package.IconUrl;
            }
        }
        public String Authors
        {
            get
            {
                return Package.Authors == null ? "" : String.Join(" ", Package.Authors);
            }
        }
        public FeedGameViewModel(IPackage package)
        {
            Package = package;
            GameManager.Get().GameListChanged += OnGameListChanged;
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
            this.OnPropertyChanged("Package");
            this.OnPropertyChanged("Installed");
            this.OnPropertyChanged("ImageUri");
            this.OnPropertyChanged("Id");
            this.OnPropertyChanged("InstallButtonText");
        }

        #region PropertyChanged
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
        #endregion PropertyChanged

        public bool Equals(FeedGameViewModel other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GameManager.Get().GameListChanged -= OnGameListChanged;
        }

        #endregion
    }
}