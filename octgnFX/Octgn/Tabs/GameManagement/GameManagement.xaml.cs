using System;
using System.Linq;
using System.Windows;
using Octgn.Utils;

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
    using Octgn.Controls;
    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.Extentions;
    using Octgn.Library;
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
                if (value.Name.Equals("Local", StringComparison.InvariantCultureIgnoreCase) ||
                    value.Name.Equals("OCTGN Official", StringComparison.InvariantCultureIgnoreCase))
                {
                    RemoveButtonEnabled = false;
                }
                else { RemoveButtonEnabled = true; }
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
                if (selectedGame != null)
                {
                    var old = selectedGame;
                    old.Dispose();
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
        {
            get { return buttonsEnabled; }
            set
            {
                buttonsEnabled = value;
                OnPropertyChanged("ButtonsEnabled");
            }
        }
        private bool removeButtonEnabled;
        public bool RemoveButtonEnabled
        {
            get { return removeButtonEnabled && buttonsEnabled; }
            set
            {
                removeButtonEnabled = value;
                OnPropertyChanged("RemoveButtonEnabled");
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

        public bool NoGamesInstalled
        {
            get
            {
                return GameManager.Get().GameCount == 0;
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
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Selected = Feeds.FirstOrDefault(x => x.Url.Equals(Paths.Get().MainOctgnFeed, StringComparison.InvariantCultureIgnoreCase));
        }

        internal void UpdatePackageList()
        {
            Dispatcher.Invoke(new Action(() => { this.ButtonsEnabled = false; }));
            var packs = GameFeedManager.Get()
                .GetPackages(Selected)
                .Where(x => x.IsAbsoluteLatestVersion)
                .OrderBy(x => x.Title)
                .GroupBy(x => x.Id)
                .Select(x => x.OrderByDescending(y => y.Version.Version).First())
                .Select(x => new FeedGameViewModel(x)).ToList();
            foreach (var package in packages.ToList())
            {
                var pack = package;
                Dispatcher.Invoke(new Action(() =>
                                                 {
                                                     packages.Remove(pack);
                                                     pack.Dispose();
                                                 }));
                //if (!packs.Contains(p)) 
                //    Dispatcher.Invoke(new Action(()=>packages.Remove(p)));
            }
            foreach (var package in packs)
            {
                var pack = package;
                Dispatcher.Invoke(new Action(() => packages.Add(pack)));
            }
            if (Selected != null)
            {
                SelectedGame =
                    Packages.FirstOrDefault(x => x.Id.Equals(Guid.Parse("844d5fe3-bdb5-4ad2-ba83-88c2c2db6d88")));
                if (SelectedGame == null)
                {
                    SelectedGame = Packages.FirstOrDefault();
                }
                OnPropertyChanged("SelectedGame");
                OnPropertyChanged("IsGameSelected");
            }
            Dispatcher.Invoke(new Action(() => { this.ButtonsEnabled = true; }));
        }

        #region Events

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Packages")
            {
                new Task(() =>
                    {
                        try
                        {
                            this.UpdatePackageList();
                        }
                        catch (Exception e)
                        {
                            Log.Warn("", e);
                        }
                    }).Start();
            }
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Selected");
            OnPropertyChanged("Packages");
            OnPropertyChanged("NoGamesInstalled");
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
                        if (this.Feeds.All(x => x.Name != f.Name))
                            Feeds.Add(f);
                    }
                }));
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            ButtonsEnabled = false;
            var dialog = new AddFeed();
            dialog.Show(DialogPlaceHolder);
            dialog.OnClose += (o, result) =>
                                  {
                                      ButtonsEnabled = true;
                                      dialog.Dispose();
                                  };
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
                    Log.Warn("Could not add " + of.FileName + " to local feed.", ex);
                    TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    Log.Warn("Could not add " + of.FileName + " to local feed.", ex);
                    TopMostMessageBox.Show(
                        "Could not add file " + of.FileName
                        + ". Please make sure it isn't in use and that you have access to it.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private bool installo8cprocessing = false;
        private void ButtonAddo8cClick(object sender, RoutedEventArgs e)
        {
            if (installo8cprocessing) return;
            installo8cprocessing = true;
            var of = new System.Windows.Forms.OpenFileDialog();
            of.Multiselect = true;
            of.Filter = "Octgn Card Package (*.o8c) |*.o8c";
            var result = of.ShowDialog();
            if (result == DialogResult.OK)
            {
                var filesToImport = (from f in of.FileNames
                                     select new ImportFile { Filename = f, Status = ImportFileStatus.Unprocessed }).ToList();
                this.ProcessTask(
                            () =>
                            {

                                foreach (var f in filesToImport)
                                {
                                    try
                                    {
                                        if (!File.Exists(f.Filename))
                                        {
                                            f.Status = ImportFileStatus.FileNotFound;
                                            f.Message = "File not found.";
                                            continue;
                                        }
                                        GameManager.Get().Installo8c(f.Filename);
                                        f.Status = ImportFileStatus.Imported;
                                        f.Message = "Installed Successfully";
                                    }
                                    catch (UserMessageException ex)
                                    {
                                        var message = ex.Message;
                                        Log.Warn(message, ex);
                                        f.Message = message;
                                        f.Status = ImportFileStatus.Error;
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = "Could not install o8c.";
                                        Log.Warn(message, ex);
                                        f.Message = message;
                                        f.Status = ImportFileStatus.Error;
                                    }
                                }
                            },
                            () =>
                            {
                                this.installo8cprocessing = false;

                                var message = "The following image packs were installed:\n\n{0}"
                                    .With(filesToImport.Where(f => f.Status == ImportFileStatus.Imported).Aggregate("",
                                                                  (current, file) =>
                                                                  current +
                                                                  "· {0}\n".With(file.SafeFileName)));
                                if (filesToImport.Any(f => f.Status != ImportFileStatus.Imported))
                                {
                                    message += "\nThe following image packs could not be installed:\n\n{0}"
                                        .With(filesToImport.Where(f => f.Status != ImportFileStatus.Imported)
                                                           .Aggregate("", (current, file) => current +
                                                                      "· {0}: {1}\n".With(file.SafeFileName, file.Message)));
                                }


                                TopMostMessageBox.Show(
                                            message,
                                            "Install Image Packs",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);

                            },
                            "Installing image pack.",
                            "Please wait while your image pack is installed. You can switch tabs if you like.");
            }
            else
            {
                installo8cprocessing = false;
            }
        }

        private WaitingDialog ProcessTask(Action action, Action completeAction, string title, string message)
        {
            ButtonsEnabled = false;
            var dialog = new WaitingDialog();
            dialog.OnClose += (o, result) =>
                                  {
                                      ButtonsEnabled = true;
                                      completeAction();
                                      dialog.Dispose();
                                  };
            dialog.Show(DialogPlaceHolder, action, title, message);
            return dialog;
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
                    TopMostMessageBox.Show(
                        "You can not install/uninstall games while you are in a game.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (WindowManager.DeckEditor != null)
                {
                    TopMostMessageBox.Show(
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
                        this.ProcessTask(
                            () =>
                            {
                                try
                                {
                                    GameManager.Get().UninstallGame(game);
                                }
                                catch (IOException ex)
                                {
                                    TopMostMessageBox.Show(
                                        "Could not uninstall the game. Please try exiting all running instances of OCTGN and try again.\nYou can also try switching feeds, and then switching back and try again.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                                catch (UnauthorizedAccessException ex)
                                {
                                    TopMostMessageBox.Show(
                                        "Could not uninstall the game. Please try exiting all running instances of OCTGN and try again.\nYou can also try switching feeds, and then switching back and try again.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("Could not fully uninstall game " + model.Package.Title, ex);
                                }
                            },
                            () => { this.installuninstallprocessing = false; },
                            "Uninstalling Game",
                            "Please wait while your game is uninstalled. You can switch tabs if you like.");
                    }
                }
                else
                {
                    this.ProcessTask(
                    () =>
                    {
                        try
                        {
                            GameManager.Get().InstallGame(model.Package);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            TopMostMessageBox.Show(
                                "Could not install the game. Please try exiting all running instances of OCTGN and try again.\nYou can also try switching feeds, and then switching back and try again.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Could not install game " + model.Package.Title, ex);
                            var res = TopMostMessageBox.Show(
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
                                    Program.LaunchUrl(model.Package.ProjectUrl.ToString());

                                }
                                catch (Exception exx)
                                {
                                    Log.Warn(
                                        "Could not launch " + model.Package.ProjectUrl.ToString() + " In default browser",
                                        exx);
                                    TopMostMessageBox.Show(
                                        "We could not open your browser. Please set a default browser and try again",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            }
                        }

                    },
                    () => { this.installuninstallprocessing = false; },
                    "Installing Game",
                    "Please wait while your game is installed. You can switch tabs if you like.");
                }

            }
            catch (Exception ex)
            {
                Log.Error("Mega Error", ex);
                TopMostMessageBox.Show(
                    "There was an error, please try again later or get in contact with us at http://www.octgn.net",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UrlMouseButtonUp(object sender, object whatever)
        {
            if (!(sender is TextBlock)) return;
            try
            {
                var url = (sender as TextBlock).DataContext as Uri;
                if (url != null)
                {
                    Program.LaunchUrl(url.OriginalString);
                }
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
