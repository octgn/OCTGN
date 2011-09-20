using System;
using System.Windows;
using Octgn.Properties;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void passwordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void passwordBox2_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            bEmail.Visibility = System.Windows.Visibility.Hidden;
            lEmail.Visibility = System.Windows.Visibility.Hidden;
            bPassword.Visibility = System.Windows.Visibility.Hidden;
            lPassword.Visibility = System.Windows.Visibility.Hidden;
            bUsername.Visibility = System.Windows.Visibility.Hidden;
            lUsername.Visibility = System.Windows.Visibility.Hidden;
            bool problem = false;
            if(String.IsNullOrWhiteSpace(tbEmail.Text))
            {
                bEmail.Visibility = System.Windows.Visibility.Visible;
                lEmail.Visibility = System.Windows.Visibility.Visible;
                lEmail.Text = "Can't be blank";
                problem = true;
            }
            if(String.IsNullOrWhiteSpace(tbPass1.Password))
            {
                bPassword.Visibility = System.Windows.Visibility.Visible;
                lPassword.Visibility = System.Windows.Visibility.Visible;
                lPassword.Text = "Can't be blank";
                problem = true;
            }
            else if(tbPass1.Password != tbPass2.Password)
            {
                bPassword.Visibility = System.Windows.Visibility.Visible;
                lPassword.Visibility = System.Windows.Visibility.Visible;
                lPassword.Text = "Passwords must match";
                problem = true;
            }
            if(String.IsNullOrWhiteSpace(tbUsername.Text))
            {
                bUsername.Visibility = System.Windows.Visibility.Visible;
                lUsername.Visibility = System.Windows.Visibility.Visible;
                lUsername.Text = "Can't be blank";
                problem = true;
            }
            if(problem)
                return;
            if(Program.lobbyClient == null)
                Program.lobbyClient = new Skylabs.Lobby.LobbyClient();
            if(!Program.lobbyClient.Connected)
                Program.lobbyClient.Connect("localhost", int.Parse(Settings.Default.ServePort));
            if(Program.lobbyClient.Connected)
            {
                Program.lobbyClient.Register(RegisterFinished, tbEmail.Text, tbPass1.Password, tbUsername.Text);
            }
        }

        private void RegisterFinished(string emailerror, string passworderror, string usernameerror)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                bool problem =false;
                if(emailerror != null)
                    if(!String.IsNullOrWhiteSpace(emailerror))
                    {
                        bEmail.Visibility = System.Windows.Visibility.Visible;
                        lEmail.Visibility = System.Windows.Visibility.Visible;
                        lEmail.Text = emailerror;
                        problem = true;
                    }
                if(passworderror != null)
                    if(!String.IsNullOrWhiteSpace(passworderror))
                    {
                        bPassword.Visibility = System.Windows.Visibility.Visible;
                        lPassword.Visibility = System.Windows.Visibility.Visible;
                        lPassword.Text = passworderror;
                        problem = true;
                    }
                if(usernameerror != null)
                    if(!String.IsNullOrWhiteSpace(usernameerror))
                    {
                        bUsername.Visibility = System.Windows.Visibility.Visible;
                        lUsername.Visibility = System.Windows.Visibility.Visible;
                        lUsername.Text = usernameerror;
                        problem = true;
                    }
                if(!problem)
                {
                    this.Close();
                }
            }));
        }
    }
}