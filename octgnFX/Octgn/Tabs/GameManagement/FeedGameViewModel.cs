namespace Octgn.Tabs.GameManagement
{
    using System;
    using System.ComponentModel;

    using NuGet;

    using Octgn.Annotations;
    using Octgn.Core.DataManagers;

    public class FeedGameViewModel : INotifyPropertyChanged,IEquatable<FeedGameViewModel>
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
                if (Equals(value, this.package))
                {
                    return;
                }
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
        public FeedGameViewModel(IPackage package)
        {
            Package = package;
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
    }
}