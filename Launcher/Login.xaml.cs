using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Properties;
using Skylabs.Lobby;

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

        public Login()
        {
            InitializeComponent();
            Program.lobbyClient = new LobbyClient();
            Program.lobbyClient.OnCaptchaRequired += new LobbyClient.HandleCaptcha(lobbyClient_OnCaptchaRequired);
            SpinnerRotate.CenterX = image2.Width / 2;
            SpinnerRotate.CenterY = image2.Height / 2;
            animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
            animationTimer.Tick += HandleAnimationTick;
            if(Settings.Default.Password != "")
            {
                passwordBox1.Password = Settings.Default.Password;
                cbSavePassword.IsChecked = true;
            }
            textBox1.Text = Settings.Default.Email;
#if(DEBUG)
            //TODO Remove this at some point
            MenuItem m = new MenuItem();
            m.Name = "menuOldMenu";
            m.Header = "Old Menu";
            m.Click += new RoutedEventHandler(menuOldMenu_Click);
            menuOctgn.Items.Add(m);
#endif
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

                bError.Visibility = Visibility.Hidden;
                bool c = Program.lobbyClient.Connected;
                if (!c)
                    c = Program.lobbyClient.Connect(Program.LobbySettings.Server, Program.LobbySettings.ServerPort);
                if (c)
                {
                    Program.lobbyClient.Login(LoginFinished, textBox1.Text, passwordBox1.Password, "", UserStatus.Online);
                }
                else
                {
                    isLoggingIn = false;
                    DoErrorMessage("Could not connect to the server.");
                }
            }
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
            Dispatcher.Invoke((Action)(() =>
            {
                isLoggingIn = false;
                Stop_Spinning();
                if(cbSavePassword.IsChecked == true)
                    Settings.Default.Password = passwordBox1.Password;
                else
                    Settings.Default.Password = "";
                Settings.Default.Email = textBox1.Text;
                Settings.Default.Save();
                if(success == LoginResult.Success)
                {
                    Program.ClientWindow = new Main();
                    Program.ClientWindow.Show();
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
            Program.Exit();
        }

        private void menuOctgn_Click(object sender, RoutedEventArgs e)
        {
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
    }
}