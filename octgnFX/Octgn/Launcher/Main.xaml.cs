using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Ribbon;
using Octgn.DeckBuilder;
using Octgn.Definitions;
using Skylabs.Lobby;
using Skylabs.Net;
using System.IO;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        public RoutedCommand DebugWindowCommand = new RoutedCommand();
        private bool                        _allowDirectNavigation = false;
        private NavigatingCancelEventArgs   _navArgs = null;
        private Duration                    _duration = new Duration(TimeSpan.FromSeconds(.5));
        private SetList _currentSetList;
        private void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if(Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;

                _navArgs = e;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 1;
                    animation0.To = 0;
                    animation0.Duration = _duration;
                    animation0.Completed += SlideCompleted;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                }));
            }
            _allowDirectNavigation = false;
        }

        private void SlideCompleted(object sender, EventArgs e)
        {
            _allowDirectNavigation = true;
            switch(_navArgs.NavigationMode)
            {
                case NavigationMode.New:
                    if(_navArgs.Uri == null)
                        frame1.Navigate(_navArgs.Content);
                    else
                        frame1.Navigate(_navArgs.Uri);
                    break;
                case NavigationMode.Back:
                    frame1.GoBack();
                    break;
                case NavigationMode.Forward:
                    frame1.GoForward();
                    break;
                case NavigationMode.Refresh:
                    frame1.Refresh();
                    break;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (ThreadStart)delegate()
                {
                    frame1.UpdateLayout();
                    UpdateLayout();
                    DoubleAnimation animation0 = new DoubleAnimation();
                    animation0.From = 0;
                    animation0.To = 1;
                    animation0.Duration = _duration;
                    frame1.BeginAnimation(OpacityProperty, animation0);
                });
        }

        public Main()
        {
            InitializeComponent();
            frame1.Navigate(new ContactList());
            DebugWindowCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            CommandBinding cb = new CommandBinding(DebugWindowCommand,
                MyCommandExecute, MyCommandCanExecute);
            this.CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.M, ModifierKeys.Control);
            InputBinding ib = new InputBinding(DebugWindowCommand, kg);
            this.InputBindings.Add(ib);
            // Insert code required on object creation below this point.
        }

        private void Ribbon_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RibbonTab tab = Ribbon.SelectedItem as RibbonTab;
            if(tab != null)
            {
                switch((String)tab.Header)
                {
                    case "Lobby":
                        frame1.Navigate(new ContactList());
                        break;
                    case "Games":
                        GameList gl = new GameList();
                        gl.OnGameClick += new EventHandler(gl_OnGameDoubleClick);
                        frame1.Navigate(gl);
                        break;
                    case "!":
                        frame1.Navigate(new NotificationList());
                        break;
                }
            }
        }

        void gl_OnGameDoubleClick(object sender, EventArgs e)
        {
            Data.Game g = sender as Data.Game;
            _currentSetList = new SetList(g);
            frame1.Navigate(_currentSetList);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
            Program.Exit();
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            if (Program.DeckEditor != null)
                Program.DeckEditor.Close();
            Program.LauncherWindow = new LauncherWindow();
            Program.LauncherWindow.Show();
            Program.ClientWindow.Close();
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);            
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            GameList gl = frame1.Content as GameList;
            gl.Install_Game();
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSetList = null;
            GameList gl = new GameList();
            gl.OnGameClick += new EventHandler(gl_OnGameDoubleClick);
            frame1.Navigate(gl);
        }

        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            if(_currentSetList != null)
            _currentSetList.Deleted_Selected();
        }

        private void bInstallSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
            _currentSetList.Install_Sets();
        }

        private void bPatchSets_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSetList != null)
            _currentSetList.Patch_Selected();
        }

        private void bDeckEditor_Click(object sender, RoutedEventArgs e)
        {
            if (Program.GamesRepository.Games.Count == 0)
            {
                MessageBox.Show("You have no game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Program.DeckEditor == null)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();
            }
            else if (Program.DeckEditor.IsVisible == false)
            {
                Program.DeckEditor = new DeckBuilderWindow();
                Program.DeckEditor.Show();                
            }
        }

        private void bAddFriend_Click(object sender, RoutedEventArgs e)
        {
            frame1.Navigate(new AddFriendPage());
        }

        private void bHost_Click(object sender, RoutedEventArgs e)
        {
            GameList gl = new GameList();
            gl.OnGameClick += new EventHandler(gl_HostGameClick);
            frame1.Navigate(gl);
        }

        void gl_HostGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                Data.Game g = sender as Data.Game;
                frame1.Navigate(new HostGameSettings(g));
            }
        }
        private void MyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            //System.Diagnostics.XmlWriterTraceListener tr = new System.Diagnostics.XmlWriterTraceListener()
            if (Program.DebugWindow == null)
            {
                Program.DebugWindow = new DWindow();
            }
            if (Program.DebugWindow.Visibility == System.Windows.Visibility.Visible)
                Program.DebugWindow.Visibility = System.Windows.Visibility.Hidden;
            else
                Program.DebugWindow.Visibility = System.Windows.Visibility.Visible;
        }

        private void bJoin_Click(object sender, RoutedEventArgs e)
        {
            HostedGameList hgl = new HostedGameList();
            hgl.OnGameClick += new EventHandler(hgl_OnGameClick);
            frame1.Navigate(hgl);
        }
        public void StartGame()
        {
            StartGame sg = frame1.Content as StartGame;
            if (sg != null)
            {
                sg.Start();
                frame1.Navigate(new HostedGameList());
            }
        }
        void hgl_OnGameClick(object sender, EventArgs e)
        {
            if (Program.PlayWindow == null)
            {
                HostedGame hg = sender as HostedGame;
                Program.IsHost = false;
                Octgn.Data.Game theGame = Program.GamesRepository.AllGames.FirstOrDefault(g => g.Id == hg.GameGuid);
                if(theGame != null)
                {
                    Program.Game = new Game(GameDef.FromO8G(theGame.Filename));
                    IPAddress[] ad = new IPAddress[0];
#if(DEBUG)
                    ad = new IPAddress[1];
                    IPAddress ip = IPAddress.Parse("127.0.0.1");
#else
                ad = Dns.GetHostAddresses("www.skylabsonline.com");
                IPAddress ip = ad[0];
#endif

                    if (ad.Length > 0)
                    {
                        Program.Client = new Networking.Client(ip, hg.Port);
                        Program.Client.Connect();
                        this.Dispatcher.Invoke(new Action(() => frame1.Navigate(new StartGame())));

                    }
                }
            }
        }
    }
}