using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Octgn.Tabs.GameManagement
{
    using System.ComponentModel;
    using System.Reflection;

    using NuGet;

    using Octgn.Annotations;
    using Octgn.Core;

    using log4net;

    /// <summary>
    /// Interaction logic for FeedGame.xaml
    /// </summary>
    public partial class FeedGame : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private Guid Id;
        private bool installed;
        private Uri imageUri;

        public static readonly DependencyProperty PackageProperty = DependencyProperty.Register(
            "Package", typeof(IPackage), typeof(FeedGame), new UIPropertyMetadata(new PropertyChangedCallback(OnPackageChanged)));

        public IPackage Package {
            get
            {
                return (IPackage)this.GetValue(PackageProperty);
            }
            set
            {
                SetValue(PackageProperty,value);
            }
        }
        public bool Installed{
            get{return this.installed;}
            set
            {
                if (value.Equals(this.installed))
                {
                    return;
                }
                this.installed = value;
                this.OnPropertyChanged("Installed");
                this.OnPropertyChanged("InstallButtonText");
            }
        }
        public string InstallButtonText
        {
            get
            {
                return Installed ? "Install" : "Uninstall";
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
        private bool isThisSelected;
        public bool IsThisSelected
        {
            get
            {
                return isThisSelected;
            }
            set
            {
                if (value == isThisSelected) return;
                value = isThisSelected;
                OnPropertyChanged("IsThisSelected");
            }
        }
        public FeedGame()
        {
            InitializeComponent();
            this.Selected += (sender, args) => IsThisSelected = true;
            this.Unselected += (sender, args) => IsThisSelected = false;
            Octgn.Core.DataManagers.GameManager.Get().GameInstalled += OnGameInstalled;
            if (Package == null || (Package != null && !Guid.TryParse(Package.Id, out Id)))
            {
                //this.Visibility = Visibility.Collapsed;
                return;
            }
            Installed = Core.DataManagers.GameManager.Get().Games.Any(x => x.Id == Id);
        }

        private void OnGameInstalled(object sender, EventArgs eventArgs)
        {
            if (Package == null) return;
            Installed = Core.DataManagers.GameManager.Get().Games.Any(x => x.Id == Id);
        }

        internal static void OnPackageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Log.Info("Called OnPackageChanged");
            var o = obj as FeedGame;
            if (o.Package != null && !Guid.TryParse(o.Package.Id, out o.Id)) return;
            o.Visibility = Visibility.Visible;
            o.Installed = Core.DataManagers.GameManager.Get().Games.Any(x => x.Id == o.Id);
            o.OnPropertyChanged("Installed");
            o.OnPropertyChanged("InstallButtonText");
            o.OnPropertyChanged("ImageUri");
        }

        #region NotifyPropertyChanged
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
        #endregion NotifyPropertyChanged
    }
}
