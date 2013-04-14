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
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using NuGet;

    using Octgn.Annotations;
    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.Extentions;
    using Octgn.Library.Exceptions;
    using Octgn.Library.Networking;

    using log4net;

    using Button = System.Windows.Controls.Button;
    using MessageBox = System.Windows.MessageBox;
    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    /// Interaction logic for GameManagement.xaml
    /// </summary>
    public partial class GameManagement : UserControl, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ObservableCollection<NamedUrl> Feeds { get; set; }
        private FeedGameViewModel selectedGame;
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
                this.OnPropertyChanged("IsGameSelected");
                this.OnPropertyChanged("SelectedGame");
            }
        }

        public FeedGameViewModel SelectedGame
        {
            get
            {
                return this.selectedGame;
            }
            set
            {
                if (Equals(value, this.selectedGame))
                {
                    return;
                }
                this.selectedGame = value;
                this.OnPropertyChanged("IsGameSelected");
                this.OnPropertyChanged("SelectedGame");
            }
        }

        public bool IsGameSelected
        {
            get
            {
                return ListBoxGames.SelectedIndex > -1;
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
                return packages;
            }
        }

        public GameManagement()
        {
            packages = new ObservableCollection<FeedGameViewModel>();
            ButtonsEnabled = true;
            if (!this.IsInDesignMode())
            {
                Feeds = new ObservableCollection<NamedUrl>(Octgn.Core.GameFeedManager.Get().GetFeeds());
                GameFeedManager.Get().OnUpdateFeedList += OnOnUpdateFeedList;
                GameManager.Get().GameListChanged += OnGameListChanged;
            }
            else Feeds = new ObservableCollection<NamedUrl>();
            InitializeComponent();
            ListBoxGames.SelectionChanged += delegate
                {
                    OnPropertyChanged("SelectedGame");
                    OnPropertyChanged("IsGameSelected");
                };
            this.PropertyChanged += OnPropertyChanged;
        }

        internal void UpdatePackageList()
        {
            Dispatcher.Invoke(new Action(() => { this.ButtonsEnabled = false; }));
            var packs = GameFeedManager.Get()
                .GetPackages(Selected)
                .Where(x=>x.IsAbsoluteLatestVersion)
                .GroupBy(x=>x.Id)
                .Select(x=>x.OrderByDescending(y=>y.Version.Version).First())
                .Select(x => new FeedGameViewModel(x)).ToList();
            foreach (var package in packages.ToList())
            {
                var pack = package;
                Dispatcher.Invoke(new Action(()=>packages.Remove(pack)));
                //if (!packs.Contains(p)) 
                //    Dispatcher.Invoke(new Action(()=>packages.Remove(p)));
            }
            foreach (var package in packs)
            {
                var pack = package;
                Dispatcher.Invoke(new Action(()=>packages.Add(pack)));
            }
            Dispatcher.Invoke(new Action(() => { this.ButtonsEnabled = true; }));
        }

        #region Events

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Packages")
            {
                new Task(this.UpdatePackageList).Start();
            }
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Selected");
            OnPropertyChanged("Packages");
            this.OnPropertyChanged("IsGameSelected");
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

        private void ButtonAddo8gClick(object sender, RoutedEventArgs e)
        {
            var of = new System.Windows.Forms.OpenFileDialog();
            of.Filter = "Octgn Game File (*.o8g) |*.o8g";
            var result = of.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!File.Exists(of.FileName)) return;
                try
                {
                    GameFeedManager.Get().AddToLocalFeed(of.FileName);
                    OnPropertyChanged("Packages");
                }
                catch (UserMessageException ex)
                {
                    Log.Warn("Could not add " + of.FileName + " to local feed.",ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Log.Warn("Could not add " + of.FileName + " to local feed.",ex);
                    MessageBox.Show(
                        "Could not add file " + of.FileName
                        + ". Please make sure it isn't in use and that you have access to it.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void ButtonAddo8cClick(object sender, RoutedEventArgs e)
        {
            var of = new System.Windows.Forms.OpenFileDialog();
            of.Filter = "Octgn Card Package (*.o8c) |*.o8c";
            var result = of.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!File.Exists(of.FileName)) return;
                try
                {
                    GameManager.Get().Installo8c(of.FileName);
                    MessageBox.Show(
                        "The image pack was installed.", "Install", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (UserMessageException ex)
                {
                    Log.Warn("Could not install o8c " + of.FileName + ".",ex);
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Log.Warn("Could not install o8c " + of.FileName + ".", ex);
                    MessageBox.Show(
                        "Could not install o8c " + of.FileName
                        + ". Please make sure it isn't in use and that you have access to it.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private bool installuninstallprocessing = false;
        private void ButtonInstallUninstallClick(object sender, RoutedEventArgs e)
        {
            if (installuninstallprocessing) return;
            installuninstallprocessing = true;
            try
            {
                if (WindowManager.PlayWindow != null)
                {
                    MessageBox.Show(
                        "You can not install/uninstall games while you are in a game.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (WindowManager.DeckEditor != null)
                {
                    MessageBox.Show(
                        "You can not install/uninstall games while you are in the deck editor.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var button = e.Source as Button;
                if (button == null || button.DataContext == null) return;
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
                            Log.Error("Could not fully uninstall game " + model.Package.Title, ex);
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
                        Log.Error("Could not install game " + model.Package.Title, ex);
                        var res =
                            MessageBox.Show(
                                "There was a problem installing " + model.Package.Title
                                + ". \n\nPlease be aware, this is not our fault. Our code is impervious and perfect. Angels get their wings every time we press enter."
                                + "\n\nDo you want to get in contact with the game developer who broke this busted game?",
                                "Error",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Exclamation);

                        if (res == MessageBoxResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(model.Package.ProjectUrl.ToString());

                            }
                            catch (Exception exx)
                            {
                                Log.Warn(
                                    "Could not launch " + model.Package.ProjectUrl.ToString() + " In default browser",
                                    exx);
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
            catch (Exception ex)
            {
                Log.Error("Mega Error", ex);
                MessageBox.Show(
                    "There was an error, please try again later or get in contact with us at http://www.octgn.net",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                installuninstallprocessing = false;
            }
        }

        private void UrlMouseButtonUp(object sender, object whatever)
        {
            if (!(sender is TextBlock)) return;
            try
            {
                System.Diagnostics.Process.Start((sender as TextBlock).Text);
            }
            catch
            {
                
            }
        }

        #endregion Events

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
