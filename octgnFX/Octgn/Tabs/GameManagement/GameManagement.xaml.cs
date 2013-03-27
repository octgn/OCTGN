using System;
using System.Linq;
using System.Windows;

namespace Octgn.Tabs.GameManagement
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using NuGet;

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
                this.OnPropertyChanged("Packages");
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

        private ObservableCollection<FeedGameViewModel> packages;
        public ObservableCollection<FeedGameViewModel> Packages
        {
            get
            {
                if (packages == null) packages = new ObservableCollection<FeedGameViewModel>();
                var packs = GameFeedManager.Get().GetPackages(Selected).Select(x=> new FeedGameViewModel(x)).ToList();
                foreach (var p in packages.ToList())
                {
                    if (!packs.Contains(p)) packages.Remove(p);
                }
                foreach (var r in packs)
                {
                    if(!packages.Contains(r))packages.Add(r);
                }
                return packages;
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
