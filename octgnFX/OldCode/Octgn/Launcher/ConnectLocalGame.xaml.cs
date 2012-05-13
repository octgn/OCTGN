using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using Octgn.Networking;
using Skylabs.Lobby.Threading;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for ConnectLocalGame.xaml
    /// </summary>
    public partial class ConnectLocalGame : Page
    {
        private string _host;
        private string _port;
        public ConnectLocalGame()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (Program.PlayWindow != null) return;
            Program.IsHost = false;
            progressBar1.Visibility = Visibility.Visible;
            _host = tbHost.Text;
            _port = tbPort.Text;
            LazyAsync.Invoke(Connect);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void Connect()
        {
            try
            {
                IPAddress c2;
                IPAddress temp;
                if (IPAddress.TryParse(_host, out temp))
                {
                    c2 = temp;
                }
                else
                {
                    var b = Dns.GetHostAddresses(_host);
                    c2 = b.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
                Program.Client = new Client(c2, Int32.Parse(_port));
                Program.Client.Connect();
                Dispatcher.Invoke(new Action(() => NavigationService.Navigate(new StartGame(true))));
            }
            catch (SocketException)
            {
                MessageBox.Show("Could not connect to " + _host, "Octgn", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a proper port number.", "Octgn", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Octgn", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            Dispatcher.Invoke(new Action(() => progressBar1.Visibility = Visibility.Hidden));
        }
    }

}
