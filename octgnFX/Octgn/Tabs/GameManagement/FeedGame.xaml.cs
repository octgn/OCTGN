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

    using NuGet;

    using Octgn.Annotations;

    /// <summary>
    /// Interaction logic for FeedGame.xaml
    /// </summary>
    public partial class FeedGame : INotifyPropertyChanged
    {
        private Guid Id;
        private IPackage package;
        private bool installed;

        public IPackage Package {
            get{return this.package;}
            set
            {
                if (Equals(value, this.package))
                {
                    return;
                }
                this.package = value;
                this.OnPropertyChanged("Package");
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
            }
        }

        public FeedGame()
        {
            InitializeComponent();
            if (!Guid.TryParse(package.Id, out Id))
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }
            Octgn.Core.DataManagers.GameManager.Get().GameInstalled += OnGameInstalled;
            Installed = Core.DataManagers.GameManager.Get().Games.Any(x => x.Id == Id);
        }

        private void OnGameInstalled(object sender, EventArgs eventArgs)
        {
            Installed = Core.DataManagers.GameManager.Get().Games.Any(x => x.Id == Id);
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
