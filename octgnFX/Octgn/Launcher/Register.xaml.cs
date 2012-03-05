using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        public Register()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            progressBar1.Visibility = Visibility.Visible;
            Program.LClient.OnRegisterComplete += LClientOnOnRegisterComplete;
            Program.LClient.BeginRegister(tbUsername.Text,tbPass1.Password);
        }

        private void LClientOnOnRegisterComplete(object sender, Client.RegisterResults results)
        {
            Program.LClient.OnRegisterComplete -= LClientOnOnRegisterComplete;
            Dispatcher.Invoke(new Action(()=>
                {
                    progressBar1.Visibility = Visibility.Hidden;
                    switch(results)
                    {
                        case Client.RegisterResults.ConnectionError:
                            lblErrors.Content = "There was a connection error. Please try again.";
                            break;
                        case Client.RegisterResults.Success:
                            MessageBox.Show("Registration Success!", "OCTGN", MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                            var l = new Login();
                            l.textBox1.Text = Program.LClient.Username;
                            l.passwordBox1.Password = Program.LClient.Password;
                            Program.LauncherWindow.Navigate(new Login());
                            break;
                        case Client.RegisterResults.UsernameTaken:
                            lblErrors.Content = "That username is already taken.";
                            break;
                        case Client.RegisterResults.UsernameInvalid:
                            lblErrors.Content = "That username is invalid.";
                            break;
                        case Client.RegisterResults.PasswordFailure:
                            lblErrors.Content = "That password is invalid.";
                            break;
                    }
                }
            ));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Program.LauncherWindow.Navigate(new Login());
        }
    }
}
