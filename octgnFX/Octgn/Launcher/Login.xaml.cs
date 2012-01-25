using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Properties;
using Octgn.DeckBuilder;
using Skylabs.Lobby;
using System.Diagnostics;
using System.Threading;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly DispatcherTimer animationTimer;
        private bool bSpin = false;
        private bool isLoggingIn = false;
        private Timer LoginTimer;

        public Login()
        {
            InitializeComponent();
            if (Program.lobbyClient != null)
            {
                Program.lobbyClient.Stop();
                Program.lobbyClient = null;
            }
            Program.lobbyClient = new LobbyClient();
            Program.lobbyClient.OnDataRecieved += new LobbyClient.DataRecieved(lobbyClient_OnDataRecieved);
            
            SpinnerRotate.CenterX = image2.Width / 2;
            SpinnerRotate.CenterY = image2.Height / 2;
            animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            animationTimer.Tick += HandleAnimationTick;
            string password = Registry.ReadValue("Password");
            if (password != null)
            {
                passwordBox1.Password = password.Decrypt();
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = Registry.ReadValue("E-Mail");            
#if(DEBUG)
            MenuItem m = new MenuItem();
            m.Name = "menuOldMenu";
            m.Header = "Old Menu";
            m.Click += new RoutedEventHandler(menuOldMenu_Click);
            menuOctgn.Items.Add(m);
#endif
        }

        void lobbyClient_OnDataRecieved(DataRecType type, object e)
        {
            if (type == DataRecType.ServerMessage)
            {
                string m = e as string;
                if (m != null && !String.IsNullOrWhiteSpace(m))
                {
                    MessageBox.Show(m,"Server Message",MessageBoxButton.OK,MessageBoxImage.Information);
                }
            }
        }

        private void Start_Spinning()
        {
            if(!bSpin && animationTimer.IsEnabled == false)
            {
                bSpin = true;
                animationTimer.Start();
            }
        }

        private void Stop_Spinning()
        {
            bSpin = false;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 10) % 360;
            if(SpinnerRotate.Angle == 0 && bSpin == false)
                animationTimer.Stop();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }
        private void DoLogin()
        {
            if (!isLoggingIn)
            {

                isLoggingIn = true;
                Start_Spinning();
                Program.LauncherWindow.Closing += new System.ComponentModel.CancelEventHandler(LauncherWindow_Closing);
                bError.Visibility = Visibility.Hidden;
                bool c = Program.lobbyClient.Connected;
                if (!c)
                {
                    UpdateLoginStatus("Connecting to server...");
                    c = Program.lobbyClient.Connect(Program.LobbySettings.Server, Program.LobbySettings.ServerPort);
                }
                if (c)
                {
                    Program.SaveLocation();
                    //TODO Sometimes it takes forever, maybe retry if it doesn't log in in like 10 seconds.
                    Program.lobbyClient.OnCaptchaRequired += new LobbyClient.HandleCaptcha(lobbyClient_OnCaptchaRequired);
                    Program.lobbyClient.Login(LoginFinished, UpdateLoginStatus,textBox1.Text, passwordBox1.Password, "", UserStatus.Online);
                }
                else
                {
                    UpdateLoginStatus("");
                    isLoggingIn = false;
                    DoErrorMessage("Could not connect to the server.");
                }
            }
        }
        void LauncherWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isLoggingIn)
            {
                e.Cancel = true;
                return;
            }
        }
        private void UpdateLoginStatus(string message)
        {
            Dispatcher.Invoke(new Action(() => lblLoginStatus.Content = message));
        }
        private void lobbyClient_OnCaptchaRequired(string Fullurl, string Imageurl)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Octgn.Controls.PopupWindowMessage pm = new Controls.PopupWindowMessage();
                Image i = new Image();
                TextBox tb = new TextBox();
                Button b = new Button();
                b.Width = 70;
                b.Height = 30;
                b.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                b.Content = "Ok";
                b.Click += new RoutedEventHandler((object o, RoutedEventArgs e) =>
                {
                    pm.HideMessage();
                });
                tb.Name = "tbCaptcha";

                i.Source = new BitmapImage(new Uri(Imageurl));
                pm.AddControl(i);
                pm.AddControl(tb);
                pm.AddControl(b);
                pm.OnPopupWindowClose += new Octgn.Controls.PopupWindowMessage.HandlePopupWindowClose(delegate(object sender, bool xClosed)
                {
                    isLoggingIn = false;
                    if(!xClosed)
                    {
                        button1_Click(null, null);
                    }

                    Stop_Spinning();
                });
                pm.ShowMessage(this.MainGrid);
            }));
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }

        private void LoginFinished(LoginResult success, DateTime BanEnd, string message)
        {
            if (success == LoginResult.WaitingForResponse)
            {
                LoginTimer = new Timer((object o) =>
                {
                    LoginFinished(LoginResult.Failure, DateTime.Now, "Please try again.");
                }, null, 10000, 10000);
                return;
            }
            Trace.TraceInformation("Login finished.");
            if (LoginTimer != null)
            {
                LoginTimer.Dispose();
                LoginTimer = null;
            }
            Dispatcher.Invoke((Action)(() =>
            {
                Program.lobbyClient.OnCaptchaRequired -= lobbyClient_OnCaptchaRequired;
                Program.LauncherWindow.Closing -= LauncherWindow_Closing;
                isLoggingIn = false;
                Stop_Spinning();
                if(success == LoginResult.Success)
                {
                    if (cbSavePassword.IsChecked == true)
                    {
                        Registry.WriteValue("Password", passwordBox1.Password.Encrypt());
                    }
                    else
                        Registry.WriteValue("Password", "");
                    Registry.WriteValue("E-Mail", textBox1.Text);
                    Registry.WriteValue("Nickname", Program.lobbyClient.Me.DisplayName);

                    Program.ClientWindow = new Main();
                    Program.ClientWindow.Show();
                    Application.Current.MainWindow = Program.ClientWindow;
                    Program.LauncherWindow.Close();
                }
                else if(success == LoginResult.Banned)
                {
                    DoErrorMessage("You have been banned until " + BanEnd.ToShortTimeString() + " on " + BanEnd.ToShortDateString());
                }
                else if(success == LoginResult.Failure)
                {
                    DoErrorMessage("Login Failed: " + message);                    
                }
            }), new object[0] { });
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoggingIn)
            {
                Program.Exit();
            }
        }

        private bool FileExists(string URL)
        {
            bool result = false;
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                try
                {
                    System.IO.Stream str = client.OpenRead(URL);
                    if (str != null) result = true; else result = false;
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        private string[] ReadUpdateXML(string URL)
        {
            string[] values = new string[2];

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(System.Net.WebRequest.Create(URL).GetResponse().GetResponseStream()))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (!reader.IsEmptyElement)
                        {
                            switch (reader.Name)
                            {
                                case "Version":
                                    values = new string[2];
                                    if (reader.Read()) { values[0] = reader.Value; }
                                    break;
                                case "Location":
                                    if (reader.Read()) { values[1] = reader.Value; }                                    
                                    break;
                            }
                        }
                    }
                }
            }
            return values;
        }

        private void menuUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (FileExists("http://www.skylabsonline.com/downloads/octgn/update.xml"))
            {
                string[] update = new string[2];
                update = ReadUpdateXML("http://www.skylabsonline.com/downloads/octgn/update.xml");

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Version local = assembly.GetName().Version;
                Version online = new Version(update[0]);
                if (online > local)
                {
                    switch (MessageBox.Show("An update is available. Would you like to download now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            System.Diagnostics.Process.Start(update[1]);
                            Program.Exit();
                            break;
                        case MessageBoxResult.No:
                            //
                            break;
                    }
                } 
            }
        }

        private void menuOctgn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void menuDeckEditor_Click(object sender, RoutedEventArgs e)
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

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            bError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void DoErrorMessage(string message)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                lError.Text = message;
                bError.Visibility = System.Windows.Visibility.Visible;
            }), new object[0] { });
        }

        private void passwordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            bError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void menuOldMenu_Click(object sender, RoutedEventArgs e)
        {
            //TODO This event and the menu need to be removed before release. This is only here for debugging purposes
            this.NavigationService.Navigate(new MainMenu());
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private void textBox1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(cbSavePassword.IsChecked == true)
            {
                cbSavePassword.IsChecked = false;
                Settings.Default.Password = "";
                Settings.Default.Save();
            }
        }

        private void passwordBox1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLogin();
            }
            else if (cbSavePassword.IsChecked == true)
            {
                cbSavePassword.IsChecked = false;
                Settings.Default.Password = "";
                Settings.Default.Save();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnDataRecieved -= lobbyClient_OnDataRecieved;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateLoginStatus("Connecting to server...");
            if (Program.lobbyClient.Connect(Program.LobbySettings.Server, Program.LobbySettings.ServerPort))
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
        public static string Decrypt(this string Text)
        {
            System.Security.Cryptography.RIPEMD160 hash = System.Security.Cryptography.RIPEMD160.Create();
            byte[] hasher;
            hasher = hash.ComputeHash(System.Text.Encoding.Unicode.GetBytes(Registry.ReadValue("Nickname") ?? "null"));
            Text = Cryptor.Decrypt(Text, BitConverter.ToString(hasher));
            return Text;
        }

        public static string Encrypt(this string Text)
        {
            // Create a hash of current nickname to use as the Cryptographic Key
            System.Security.Cryptography.RIPEMD160 hash = System.Security.Cryptography.RIPEMD160.Create();
            byte[] hasher;
            hasher = hash.ComputeHash(System.Text.Encoding.Unicode.GetBytes(Program.lobbyClient.Me.DisplayName));
            return Cryptor.Encrypt(Text, BitConverter.ToString(hasher));
        }
    }
}