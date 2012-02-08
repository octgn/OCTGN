using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using Octgn.Controls;
using Octgn.DeckBuilder;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        private readonly DispatcherTimer _animationTimer;
        private Timer _loginTimer;
        private bool _bSpin;
        private bool _isLoggingIn;

        public Login()
        {
            InitializeComponent();
            if (Program.LobbyClient != null)
            {
                Program.LobbyClient.Stop();
                Program.LobbyClient = null;
            }
            Program.LobbyClient = new LobbyClient();
            Program.LobbyClient.OnDataRecieved += lobbyClient_OnDataRecieved;

            SpinnerRotate.CenterX = image2.Width/2;
            SpinnerRotate.CenterY = image2.Height/2;
            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
                                 {Interval = new TimeSpan(0, 0, 0, 0, 100)};
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            _animationTimer.Tick += HandleAnimationTick;
            string password = SimpleConfig.ReadValue("Password");
            if (password != null)
            {
                passwordBox1.Password = password.Decrypt();
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = SimpleConfig.ReadValue("E-Mail");
        }

        private static void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if (type != DataRecType.ServerMessage) return;
            var m = e as string;
            if (m != null && !String.IsNullOrWhiteSpace(m))
            {
                MessageBox.Show(m, "Server Message", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

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

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            if (_isLoggingIn) return;
            _isLoggingIn = true;
            StartSpinning();
            Program.LauncherWindow.Closing += LauncherWindowClosing;
            bError.Visibility = Visibility.Hidden;
            bool c = Program.LobbyClient.Connected;
            if (!c)
            {
                UpdateLoginStatus("Connecting to server...");
                c = Program.LobbyClient.Connect(Program.LobbySettings.Server, Program.LobbySettings.ServerPort);
            }
            if (c)
            {
                Program.SaveLocation();
                //TODO Sometimes it takes forever, maybe retry if it doesn't log in in like 10 seconds.
                Program.LobbyClient.OnCaptchaRequired += lobbyClient_OnCaptchaRequired;
                Program.LobbyClient.Login(LoginFinished, UpdateLoginStatus, textBox1.Text, passwordBox1.Password, "",
                                          UserStatus.Online);
            }
            else
            {
                UpdateLoginStatus("");
                _isLoggingIn = false;
                DoErrorMessage("Could not connect to the server.");
            }
        }

        private void LauncherWindowClosing(object sender, CancelEventArgs e)
        {
            if (_isLoggingIn)
            {
                e.Cancel = true;
            }
        }

        private void UpdateLoginStatus(string message)
        {
            Dispatcher.Invoke(new Action(() => lblLoginStatus.Content = message));
        }

        private void lobbyClient_OnCaptchaRequired(string fullurl, string imageurl)
        {
            Dispatcher.Invoke((Action) (() =>
                                            {
                                                var pm = new PopupWindowMessage();
                                                var i = new Image();
                                                var tb = new TextBox();
                                                var b = new Button
                                                            {
                                                                Width = 70,
                                                                Height = 30,
                                                                HorizontalAlignment = HorizontalAlignment.Right,
                                                                Content = "Ok"
                                                            };

                                                b.Click += (o, e) => { pm.HideMessage(); };
                                                tb.Name = "tbCaptcha";

                                                i.Source = new BitmapImage(new Uri(imageurl));
                                                pm.AddControl(i);
                                                pm.AddControl(tb);
                                                pm.AddControl(b);
                                                pm.OnPopupWindowClose += delegate(object sender, bool xClosed)
                                                                             {
                                                                                 _isLoggingIn = false;
                                                                                 if (!xClosed)
                                                                                 {
                                                                                     Button1Click(null, null);
                                                                                 }

                                                                                 StopSpinning();
                                                                             };
                                                pm.ShowMessage(MainGrid);
                                            }));
        }

        private void WebBrowser1Navigated(object sender, NavigationEventArgs e)
        {
            //if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }

        private void LoginFinished(LoginResult success, DateTime banEnd, string message)
        {
            if (success == LoginResult.WaitingForResponse)
            {
                _loginTimer =
                    new Timer(o => { LoginFinished(LoginResult.Failure, DateTime.Now, "Please try again."); },
                              null, 10000, 10000);
                return;
            }
            Trace.TraceInformation("Login finished.");
            if (_loginTimer != null)
            {
                _loginTimer.Dispose();
                _loginTimer = null;
            }
            Dispatcher.Invoke((Action) (() =>
                                            {
                                                Program.LobbyClient.OnCaptchaRequired -= lobbyClient_OnCaptchaRequired;
                                                Program.LauncherWindow.Closing -= LauncherWindowClosing;
                                                _isLoggingIn = false;
                                                StopSpinning();
                                                switch (success)
                                                {
                                                    case LoginResult.Success:
                                                        SimpleConfig.WriteValue("Password",
                                                                                cbSavePassword.IsChecked == true
                                                                                    ? passwordBox1.Password.Encrypt()
                                                                                    : "");
                                                        SimpleConfig.WriteValue("E-Mail", textBox1.Text);
                                                        SimpleConfig.WriteValue("Nickname",
                                                                                Program.LobbyClient.Me.DisplayName);
                                                        Program.ClientWindow = new Main();
                                                        Program.ClientWindow.Show();
                                                        Application.Current.MainWindow = Program.ClientWindow;
                                                        Program.LauncherWindow.Close();
                                                        break;
                                                    case LoginResult.Banned:
                                                        DoErrorMessage("You have been banned until " +
                                                                       banEnd.ToShortTimeString() + " on " +
                                                                       banEnd.ToShortDateString());
                                                        break;
                                                    case LoginResult.Failure:
                                                        DoErrorMessage("Login Failed: " + message);
                                                        break;
                                                }
                                            }), new object[] {});
        }

        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            if (!_isLoggingIn)
            {
                Program.Exit();
            }
        }

        private static bool FileExists(string url)
        {
            bool result;
            using (var client = new WebClient())
            {
                try
                {
                    Stream str = client.OpenRead(url);
                    result = str != null;
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        private static string[] ReadUpdateXML(string url)
        {
            var values = new string[2];

            using (XmlReader reader = XmlReader.Create(WebRequest.Create(url).GetResponse().GetResponseStream()))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement()) continue;
                    if (reader.IsEmptyElement) continue;
                    switch (reader.Name)
                    {
                        case "Version":
                            values = new string[2];
                            if (reader.Read())
                            {
                                values[0] = reader.Value;
                            }
                            break;
                        case "Location":
                            if (reader.Read())
                            {
                                values[1] = reader.Value;
                            }
                            break;
                    }
                }
            }
            return values;
        }

        private void MenuUpdateClick(object sender, RoutedEventArgs e)
        {
            if (!FileExists("http://www.skylabsonline.com/downloads/octgn/update.xml")) return;
            string[] update = ReadUpdateXML("http://www.skylabsonline.com/downloads/octgn/update.xml");

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version local = assembly.GetName().Version;
            var online = new Version(update[0]);
            if (online <= local) return;
            switch (
                MessageBox.Show("An update is available. Would you like to download now?", "Update Available",
                                MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    Process.Start(update[1]);
                    Program.Exit();
                    break;
                case MessageBoxResult.No:
                    //
                    break;
            }
        }

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

        private void TextBox1TextChanged(object sender, TextChangedEventArgs e)
        {
            bError.Visibility = Visibility.Hidden;
        }

        private void DoErrorMessage(string message)
        {
            Dispatcher.Invoke((Action) (() =>
                                            {
                                                lError.Text = message;
                                                bError.Visibility = Visibility.Visible;
                                            }), new object[] {});
        }

        private void PasswordBox1PasswordChanged(object sender, RoutedEventArgs e)
        {
            bError.Visibility = Visibility.Hidden;
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private void TextBox1KeyUp(object sender, KeyEventArgs e)
        {
            if (cbSavePassword.IsChecked != true) return;
            cbSavePassword.IsChecked = false;
            SimpleConfig.WriteValue("Password", "");
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
                SimpleConfig.WriteValue("Password", "");
            }
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            UpdateLoginStatus("Connecting to server...");
            if (Program.LobbyClient.Connect(Program.LobbySettings.Server, Program.LobbySettings.ServerPort))
            {
                UpdateLoginStatus("Server available");
            }
            else
            {
                UpdateLoginStatus("");
                DoErrorMessage("Server Unavailable");
            }
        }
    }

    public static class ExtensionMethods
    {
        public static string Decrypt(this string text)
        {
            RIPEMD160 hash = RIPEMD160.Create();
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(SimpleConfig.ReadValue("Nickname") ?? "null"));
            text = Cryptor.Decrypt(text, BitConverter.ToString(hasher));
            return text;
        }

        public static string Encrypt(this string text)
        {
            // Create a hash of current nickname to use as the Cryptographic Key
            RIPEMD160 hash = RIPEMD160.Create();
            byte[] hasher = hash.ComputeHash(Encoding.Unicode.GetBytes(Program.LobbyClient.Me.DisplayName));
            return Cryptor.Encrypt(text, BitConverter.ToString(hasher));
        }
    }
}