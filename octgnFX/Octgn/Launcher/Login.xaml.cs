using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using LinqToTwitter;
using Octgn.Controls;
using Octgn.DeckBuilder;
using Octgn.Extentions;
using Octgn.Networking;
using Skylabs.Lobby;
using Octgn.Definitions;
using Skylabs.Lobby.Threading;
using Client = Octgn.Networking.Client;
using Octgn.Data;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        private readonly DispatcherTimer _animationTimer;
        private bool _bSpin;
        private bool _isLoggingIn;
        private Timer _loginTimer;
        private bool _inLoginDone = false;
        public Login()
        {
            InitializeComponent();

            SpinnerRotate.CenterX = image2.Width/2;
            SpinnerRotate.CenterY = image2.Height/2;
            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
                                  {Interval = new TimeSpan(0, 0, 0, 0, 100)};
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            _animationTimer.Tick += HandleAnimationTick;
            string password = Prefs.Password;
            if (password != null)
            {
                passwordBox1.Password = password.Decrypt();
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = Prefs.Username;
            Program.LobbyClient.OnStateChanged += (sender , state) => UpdateLoginStatus(state);
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
            LazyAsync.Invoke(GetTwitterStuff);
        }

        #region News Feed
            private void GetTwitterStuff()
            {

                try
                {
                    LinqToTwitter.TwitterContext tc = new TwitterContext();

                    var tweets =
                        (from tweet in tc.Status
                         where tweet.Type == StatusType.User
                               && tweet.ScreenName == "octgn_official"
                               && tweet.Count == 5
                         select tweet).ToList();
                    Dispatcher.Invoke(new Action(() => ShowTwitterStuff(tweets)));
                }
                catch (TwitterQueryException)
                {
                    Dispatcher.Invoke(new Action(()=>textBlock5.Text="Could not retrieve news feed."));
                }         
                catch(Exception)
                {
                    Dispatcher.Invoke(new Action(() => textBlock5.Text = "Could not retrieve news feed."));
                }
            }
            private void ShowTwitterStuff(List<Status> tweets )
            {
                textBlock5.HorizontalAlignment = HorizontalAlignment.Stretch;
                textBlock5.Inlines.Clear();
                textBlock5.Text = "";
                foreach( var tweet in tweets)
                {
                    Inline dtime =
                        new Run(tweet.CreatedAt.ToShortDateString()
                                + tweet.CreatedAt.ToShortTimeString());
                    dtime.Foreground =
                        new SolidColorBrush(Colors.Khaki);
                    textBlock5.Inlines.Add(dtime);
                    textBlock5.Inlines.Add("\n");
                    var inlines = AddTweetText(tweet.Text).Inlines.ToArray();
                    foreach(var i in inlines)
                        textBlock5.Inlines.Add(i);     
                    textBlock5.Inlines.Add("\n\n");
                }
                //Dispatcher.BeginInvoke(new Action(StartTwitterAnim) , DispatcherPriority.Background);
            }
            private Paragraph AddTweetText(string text)
            {
                var ret = new Paragraph();
                var words = text.Split(' ');
                var b = new SolidColorBrush(Colors.White);
                foreach(var inn in words.Select(word=>StringToRun(word,b)))
                {
                    if(inn != null)
                        ret.Inlines.Add(inn);
                    ret.Inlines.Add(" ");
                }
                return ret;
            }
            public Inline StringToRun(String s, Brush b)
            {
                Inline ret = null;
                const string strUrlRegex =
                    "(?i)\\b((?:[a-z][\\w-]+:(?:/{1,3}|[a-z0-9%])|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))";
                var reg = new Regex(strUrlRegex);
                s = s.Trim();
                //b = Brushes.Black;
                Inline r = new Run(s);
                if(reg.IsMatch(s))
                {
                    b = Brushes.LightBlue;
                    var h = new Hyperlink(r);
                    h.Foreground = new SolidColorBrush(Colors.LawnGreen);
                    h.RequestNavigate += HOnRequestNavigate;
                    try
                    {
                        h.NavigateUri = new Uri(s);
                    }
                    catch(UriFormatException)
                    {
                        s = "http://" + s;
                        try
                        {
                            h.NavigateUri = new Uri(s);
                        }
                        catch(Exception)
                        {
                            r.Foreground = b;
                            //var ul = new Underline(r);
                        }
                    }
                    ret = h;
                }
                else
                    ret = new Run(s){Foreground = b};
                return ret;
            }

            private void HOnRequestNavigate(object sender , RequestNavigateEventArgs e) 
            {
 
                var hl = (Hyperlink) sender;
                string navigateUri = hl.NavigateUri.ToString();
                try
                {
                    Process.Start(new ProcessStartInfo(navigateUri));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                e.Handled = true;
            }
        #endregion

        #region LoginStuff
            private void StartSpinning()
            {
                if (_bSpin || _animationTimer.IsEnabled) return;
                _bSpin = true;
                _animationTimer.Start();
            }

            private void StopSpinning()
            {
                _bSpin = false;
            }

            private void HandleAnimationTick(object sender, EventArgs e)
            {
                SpinnerRotate.Angle = (SpinnerRotate.Angle + 10)%360;
                if (Math.Abs(SpinnerRotate.Angle - 0) < double.Epsilon && _bSpin == false)
                    _animationTimer.Stop();
            }

            void LobbyClientOnLoginComplete(object sender, Skylabs.Lobby.Client.LoginResults results)
            {
                
                switch (results)
                {
                    case Skylabs.Lobby.Client.LoginResults.ConnectionError:
                        Dispatcher.Invoke(new Action(() =>
                                                         {
                                                            UpdateLoginStatus("");
                                                            _isLoggingIn = false;
                                                            DoErrorMessage("Could not connect to the server.");
                                                            StopSpinning();    
                                                         }));

                        break;
                    case Skylabs.Lobby.Client.LoginResults.Success:
                        LoginFinished(Skylabs.Lobby.Client.LoginResult.Success, DateTime.Now,"");
                        break;
                    case Skylabs.Lobby.Client.LoginResults.Failure:
                        LoginFinished(Skylabs.Lobby.Client.LoginResult.Failure, DateTime.Now,"Username/Password Incorrect.");
                        break;
                }
                _isLoggingIn = false;
            }

            private void DoLogin()
            {
                if (_isLoggingIn) return;
                _loginTimer =
                    new Timer(
                        o =>
                        {
                            Program.LobbyClient.Stop();
                            LoginFinished(Skylabs.Lobby.Client.LoginResult.Failure , DateTime.Now ,
                                          "Please try again.");
                        } ,
                        null , 10000 , System.Threading.Timeout.Infinite);
                _isLoggingIn = true;
                StartSpinning();
                bError.Visibility = Visibility.Hidden;
                Program.LobbyClient.BeginLogin(textBox1.Text,passwordBox1.Password);
            }


            private void UpdateLoginStatus(string message)
            {
                Dispatcher.Invoke(new Action(() => lblLoginStatus.Content = message));
            }

            private void LoginFinished(Skylabs.Lobby.Client.LoginResult success, DateTime banEnd, string message)
            {
                if (_inLoginDone) return;
                _inLoginDone = true;
                Trace.TraceInformation("Login finished.");
                if (_loginTimer != null)
                {
                    _loginTimer.Dispose();
                    _loginTimer = null;
                }
                Dispatcher.Invoke((Action) (() =>
                                                {
                                                    Program.LauncherWindow.Closing -= LauncherWindowClosing;
                                                    _isLoggingIn = false;
                                                    StopSpinning();
                                                    switch (success)
                                                    {
                                                        case Skylabs.Lobby.Client.LoginResult.Success:
                                                            Prefs.Password = cbSavePassword.IsChecked == true
                                                                                 ? passwordBox1.Password.Encrypt()
                                                                                 : "";
                                                            Prefs.Username = textBox1.Text;
                                                            Prefs.Nickname = textBox1.Text;
                                                            Program.MainWindow = new Windows.Main();
                                                            Program.MainWindow.Show();
                                                            Application.Current.MainWindow = Program.MainWindow;
                                                            Program.LauncherWindow.Close();
                                                            break;
                                                        case Skylabs.Lobby.Client.LoginResult.Banned:
                                                            DoErrorMessage("You have been banned until " +
                                                                           banEnd.ToShortTimeString() + " on " +
                                                                           banEnd.ToShortDateString());
                                                            break;
                                                        case Skylabs.Lobby.Client.LoginResult.Failure:
                                                            DoErrorMessage("Login Failed: " + message);
                                                            break;
                                                    }
                                                    _inLoginDone = false;
                                                }), new object[] {});
            }

            private void DoErrorMessage(string message)
            {
                Dispatcher.Invoke((Action) (() =>
                                                {
                                                    lError.Text = message;
                                                    bError.Visibility = Visibility.Visible;
                                                }), new object[] {});
            }
        #endregion

        #region Offline Gaming
            private void MenuOfflineClick(object sender, RoutedEventArgs e)
            {
                var g = new GameList();
                var sg = new StartGame();
                g.Row2.Height = new GridLength(25);
                g.btnCancel.Click += delegate(object o, RoutedEventArgs args)
                                         {
                                             Program.LauncherWindow.NavigationService.GoBack();
                                         };
                g.OnGameClick += GOnOnGameClick;
                Program.LauncherWindow.Navigate(g);
            }

            private void GOnOnGameClick(object sender, EventArgs eventArgs)
            {
                var hg = sender as Octgn.Data.Game;
                if (hg == null || Program.PlayWindow != null)
                {
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }
                var hostport = 5000;
                while (!Skylabs.Lobby.Networking.IsPortAvailable(hostport))
                    hostport++;
                var hs = new HostedGame(hostport, hg.Id, hg.Version, "LocalGame", "", null,true);
                hs.HostedGameDone += hs_HostedGameDone;
                if (!hs.StartProcess())
                {
                    hs.HostedGameDone -= hs_HostedGameDone;
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }

                Program.IsHost = true;
                Data.Game theGame =
                    Program.GamesRepository.Games.FirstOrDefault(g => g.Id == hg.Id);
                if (theGame == null) return;
                Program.Game = new Game(GameDef.FromO8G(theGame.FullPath),true);

                var ad = new IPAddress[1];
                IPAddress ip = IPAddress.Parse("127.0.0.1");

                if (ad.Length <= 0) return;
                try
                {
                    Program.Client = new Client(ip, hostport);
                    Program.Client.Connect();
                    Dispatcher.Invoke(new Action(() => Program.LauncherWindow.NavigationService.Navigate(new StartGame(true){Width = 400})));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
            
            void hs_HostedGameDone(object sender, EventArgs e)
            {
                //throw new NotImplementedException();
            }
            private void GOoffConnOnGameClick(object sender, EventArgs eventArgs)
            {
                var hg = sender as Octgn.Data.Game;
                if (hg == null || Program.PlayWindow != null)
                {
                    Program.LauncherWindow.NavigationService.Navigate(new Login());
                    return;
                }
                Program.IsHost = false;
                Data.Game theGame =
                    Program.GamesRepository.Games.FirstOrDefault(g => g.Id == hg.Id);
                if (theGame == null)
                {
                    Program.LauncherWindow.Navigate(new Login());
                    return;
                }
                Program.Game = new Game(GameDef.FromO8G(theGame.FullPath),true);
                Program.LauncherWindow.Navigate(new ConnectLocalGame());
            }

            private void MenuOfflineConnectClick(object sender, RoutedEventArgs e)
            {
                var g = new GameList();
                g.Row2.Height = new GridLength(25);
                g.btnCancel.Click += delegate(object o, RoutedEventArgs args)
                {
                    Program.LauncherWindow.NavigationService.GoBack();
                };
                g.OnGameClick += GOoffConnOnGameClick;
                Program.LauncherWindow.Navigate(g);
                
            }
        #endregion

        #region UI Events
            private void Button1Click(object sender, RoutedEventArgs e) { DoLogin(); }
            private void MenuDeckEditorClick(object sender, RoutedEventArgs e)
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
            private void menuAboutUs_Click(object sender, RoutedEventArgs e)
            {
                Program.LauncherWindow.Navigate(new Windows.AboutWindow());
            }
            private void menuHelp_Click(object sender, RoutedEventArgs e)
            {
                Process.Start("https://github.com/kellyelton/OCTGN/wiki");
            }
            private void menuBug_Click(object sender, RoutedEventArgs e)
            {
                Process.Start("https://github.com/kellyelton/OCTGN/issues");
            }
            private void TextBox1TextChanged(object sender, TextChangedEventArgs e){bError.Visibility = Visibility.Hidden;}
            private void PasswordBox1PasswordChanged(object sender, RoutedEventArgs e){bError.Visibility = Visibility.Hidden;}
            private void btnRegister_Click(object sender, RoutedEventArgs e){Program.LauncherWindow.Navigate(new Register());}
            private void TextBox1KeyUp(object sender, KeyEventArgs e)
            {
                cbSavePassword.IsChecked = false;
            }
            private void PasswordBox1KeyUp(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    DoLogin();
                }
                else if (cbSavePassword.IsChecked == true)
                {
                    cbSavePassword.IsChecked = false;
                }
            }
        #endregion

        #region Window stuff
            private void PageUnloaded(object sender, RoutedEventArgs e)
            {
                Program.LobbyClient.OnLoginComplete -= LobbyClientOnLoginComplete;
            }

            private void PageLoaded(object sender, RoutedEventArgs e)
            {
                //TODO Check for server here
                menuInstallOnBoot.IsChecked = Prefs.InstallOnBoot;
            }
            private void LauncherWindowClosing(object sender, CancelEventArgs e){if (_isLoggingIn)e.Cancel = true;}
            private void MenuExitClick(object sender, RoutedEventArgs e){if (!_isLoggingIn)Program.Exit();}
        #endregion            

            private void menuCD_Click(object sender, RoutedEventArgs e)
            {
                System.Windows.Forms.FolderBrowserDialog pf = new FolderBrowserDialog();
                pf.SelectedPath = GamesRepository.BasePath;
                var dr = pf.ShowDialog();
                if(dr == DialogResult.OK)
                {
                    if(pf.SelectedPath.ToLower() != GamesRepository.BasePath.ToLower())
                    {
                        Prefs.DataDirectory = pf.SelectedPath;
                        var asm = System.Reflection.Assembly.GetExecutingAssembly();
                        var thispath = asm.Location;
                        Program.Exit();
                        Process.Start(thispath);
                        /*Application.Current.Exit += delegate(object o , ExitEventArgs args)
                                                    {
                                                        Process.Start(thispath);

                                                    };*/
                    }
                }
            }

            private void menuInstallOnBoot_Checked(object sender, RoutedEventArgs e) { Prefs.InstallOnBoot = menuInstallOnBoot.IsChecked; }

            private void menuInstallOnBoot_Unchecked(object sender, RoutedEventArgs e)
            {
                Prefs.InstallOnBoot = menuInstallOnBoot.IsChecked;
            }

    }

}