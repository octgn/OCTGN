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
    using System.Collections.ObjectModel;

    using Octgn.Extentions;
    using Octgn.Library.Networking;

    /// <summary>
    /// Interaction logic for GameManagement.xaml
    /// </summary>
    public partial class GameManagement : UserControl
    {
        public ObservableCollection<NamedUrl> Feeds { get; set; } 
        public GameManagement()
        {
            if(!this.IsInDesignMode())
                Feeds = new ObservableCollection<NamedUrl>(Octgn.Core.GameFeedManager.Get().GetFeeds());
            else
                Feeds = new ObservableCollection<NamedUrl>();
            InitializeComponent();
        }
    }
}
