using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
    using System.ComponentModel;
    using System.Windows.Forms;

    using Octgn.Annotations;
    using Octgn.Core;
    using Octgn.Extentions;
    using Octgn.Library.Networking;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    /// Interaction logic for GameManagement.xaml
    /// </summary>
    public partial class GameManagement : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<NamedUrl> Feeds { get; set; }

        private NamedUrl selected;
        public NamedUrl Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                if (Equals(value, this.selected))
                {
                    return;
                }
                this.selected = value;
                this.OnPropertyChanged("Selected");
            }
        }

        private bool buttonsEnabled;
        public bool ButtonsEnabled
            {get{return buttonsEnabled;}
            set
            {
                buttonsEnabled = value;
                OnPropertyChanged("ButtonsEnabled");
            }
        }
        public GameManagement()
        {
            ButtonsEnabled = true;
            if (!this.IsInDesignMode())
            {
                Feeds = new ObservableCollection<NamedUrl>(Octgn.Core.GameFeedManager.Get().GetFeeds());
                GameFeedManager.Get().OnUpdateFeedList += OnOnUpdateFeedList;
            }
            else Feeds = new ObservableCollection<NamedUrl>();
            InitializeComponent();
        }

        private void OnOnUpdateFeedList(object sender, EventArgs eventArgs)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    var realList = GameFeedManager.Get().GetFeeds().ToList();
                    foreach (var f in Feeds.ToArray())
                    {
                        if (realList.All(x => x.Name != f.Name)) Feeds.Remove(f);
                    }
                    foreach (var f in realList)
                    {
                        if(this.Feeds.All(x => x.Name != f.Name))
                            Feeds.Add(f);
                    }
            }));
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            ButtonsEnabled = false;
            var dialog = new AddFeed();
            dialog.Show(DialogPlaceHolder);
            dialog.OnClose += (o, result) => ButtonsEnabled = true;
        }

        private void ButtonRemoveClick(object sender, RoutedEventArgs e)
        {
            if (Selected == null) return;
            GameFeedManager.Get().RemoveFeed(Selected.Name);
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
