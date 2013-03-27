using System;
using System.Linq;
using System.Windows;

namespace Octgn.Tabs.GameManagement
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Windows.Controls;

    using NuGet;

    using Octgn.Annotations;
    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.Extentions;
    using Octgn.Library.Networking;

    using log4net;

    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    /// Interaction logic for GameManagement.xaml
    /// </summary>
    public partial class GameManagement : UserControl, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                GameManager.Get().GameListChanged += OnGameListChanged;
            }
            else Feeds = new ObservableCollection<NamedUrl>();
            InitializeComponent();
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Selected");
            OnPropertyChanged("Packages");
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

        private void ButtonInstallUninstallClick(object sender, RoutedEventArgs e)
        {
            var button = e.Source as Button;
            if(button == null || button.DataContext == null)return;
            var model = button.DataContext as FeedGameViewModel;
            if (model == null) return;
            if (model.Installed)
            {
                var game = GameManager.Get().GetById(model.Id);
                if (game != null)
                {
                    try
                    {
                        GameManager.Get().UninstallGame(game);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not fully uninstall game " + model.Package.Title,ex);
                    }
                }
            }
            else
            {
                try
                {
                    GameManager.Get().InstallGame(model.Package);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not install game " + model.Package.Title,ex);
                    var res = MessageBox.Show(
                        "There was a problem installing " + model.Package.Title
                        + ". \n\nPlease be aware, this is not our fault. Our code is impervious and perfect. Angels get their wings every time we press enter."
                        +"\n\nDo you want to get in contact with the game developer who broke this busted game?",
                        "Error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(model.Package.ProjectUrl.ToString());

                        }
                        catch(Exception exx)
                        {
                            Log.Warn("Could not launch " + model.Package.ProjectUrl.ToString() + " In default browser",exx);
                            MessageBox.Show(
                                "We could not open your browser. Please set a default browser and try again",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }

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

    }
}
