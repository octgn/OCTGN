using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Octgn.Properties;

namespace Octgn.Launcher
{
	public sealed partial class Serve : Page
	{
		private bool isStarting = false;
    private WebClient webClient = new WebClient();
    private static string externalIPCache = null;    // cache, most ip services refuse to repeteadly answer requests (bots protection).

    public Serve()
		{
			InitializeComponent();
			portBox.Text = Settings.Default.ServePort;
      nickBox.Text = Settings.Default.NickName;
			GetIPAddresses();
		}

		private void Start(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			if (isStarting) return; // prevents double-click and such
			if (gameSelector.Game == null) return;

      string nick = nickBox.Text.Trim();
      if (nick.Length == 0)
      {
        MessageBox.Show("A nickname is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      if (nick != Settings.Default.NickName)
      {
        Settings.Default.NickName = nick;
        Settings.Default.Save();
      }

      int port;
      if (!int.TryParse(portBox.Text, out port))
      {
        MessageBox.Show("Invalid port number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

			Program.Game = gameSelector.Game;
			if (!Program.Game.Definition.CheckVersion())
				return;

      if (!v6Box.IsChecked.Value && !v4Box.IsChecked.Value) return;

      bool isIPv6 = v6Box.IsChecked.Value;
			isStarting = true;
			// Open a server
			Program.Server = new Server.Server(port, isIPv6, Program.Game.Definition.Id, Program.Game.Definition.Version);
			// Creates a client and connect to the server
			Program.Client = new Networking.Client(isIPv6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, int.Parse(portBox.Text));
			Program.Client.Connect();
			// Show the start game window
			NavigationService.Navigate(new StartGame());
		}

		#region IP display

    private void GetIPAddresses()
    {
      GetIPv6Addresses();
      GetIPv4Addresses();
    }

	private void GetIPv6Addresses()
	{
      IEnumerable<IPAddress> addresses;
      try
      {
        addresses = IPGlobalProperties.GetIPGlobalProperties().GetUnicastAddresses().Select(ua => ua.Address); 
      }
      catch (PlatformNotSupportedException)
      {
        // GetUnicastAddresses doesn't work on Win XP. This code can be removed when XP support is dropped
        addresses = from netInterface in NetworkInterface.GetAllNetworkInterfaces()
                    where netInterface.Supports(NetworkInterfaceComponent.IPv6)
                    from unicast in netInterface.GetIPProperties().UnicastAddresses
                    select unicast.Address;
      }
      var ips = from ip in addresses
                // Keep only IPv6 addresses
                where ip.AddressFamily == AddressFamily.InterNetworkV6
                // Which are global (not local sites or links)
                && ip.ScopeId == 0
                // Exclude the Loopback IP as well
                && !IPAddress.IsLoopback(ip)
                // Print Teredo address first (for NAT traversal)
                orderby ip.IsIPv6Teredo descending
                select ip;
      ipList.ItemsSource = ips;
      if (!ipList.HasItems)
      {
          noIpLabel.Visibility = Visibility.Visible;
          v4Box.IsChecked = true; //check the ipv4 box.
          v6Box.IsEnabled = false; //disable the ipv6 box.
      }
	}

    private void GetIPv4Addresses()
    {
      // Grab the external IP
      if (externalIPCache != null)
      {
        webIPBlock.DataContext = externalIPCache;
        webIPText.FontStyle = FontStyles.Normal;
      }
      else
      {
        webClient.Headers.Add("User-Agent", "OCTGN");
        webClient.DownloadStringCompleted += GetExternalIPv6Address;
        webClient.DownloadStringAsync(new Uri("http://www.octgn.net/my-ip.php"));
        Unloaded += delegate { webClient.CancelAsync(); };
      }
      
      // Grab the local IP
      IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
      ipv4List.ItemsSource = ips.Select(ip => ip.ToString())
                                .Where(ip => Regex.IsMatch(ip, @"^\d+\.\d+\.\d+\.\d+$"));

      if (externalIPCache != null)
        UpdateNATStatus();
    }

    private void GetExternalIPv6Address(object sender, DownloadStringCompletedEventArgs e)
    {
      if (e.Cancelled)
      { webIPBlock.DataContext = "Cancelled"; return; }
      if (e.Error != null)
      { webIPBlock.DataContext = "External service unavailable"; return; }

      Match m = Regex.Match(e.Result, @"(\d+\.\d+\.\d+\.\d+)");
      if (!m.Success)
      { webIPBlock.DataContext = "Error: no IP returned"; return; }

      webIPBlock.DataContext = externalIPCache = m.ToString();
      webIPText.FontStyle = FontStyles.Normal;
      UpdateNATStatus();
    }

    private void UpdateNATStatus()
    {
      if (!ipv4List.ItemsSource.Cast<string>().Contains(externalIPCache))
        natWarning.Visibility = Visibility.Visible;
    }

		private void CopyIP(object sender, RoutedEventArgs e)
		{
      var link = sender as System.Windows.Documents.Hyperlink;
      var ipString = link.DataContext as string;
      if (ipString == null)
      {
        var ip = link.DataContext as IPAddress;
        ipString = ip.ToString();
      }
      SaferSetClipboard(ipString);
			e.Handled = true;
		}

    // Workaround a .NET bug in some scenarios like Terminal Services
    // Details are here: 
    // http://stackoverflow.com/questions/68666/clipbrdecantopen-error-when-setting-the-clipboard-from-net/68857
    private void SaferSetClipboard(string text)
    {
      for (int i = 0; i < 10; ++i)
      {
        try
        {
          Clipboard.SetText(text);
          return;
        }
        catch { }
        System.Threading.Thread.Sleep(100);
      }
    }

    #endregion

    private void GoToWebSite(object sender, RoutedEventArgs e)
		{
      var link = sender as System.Windows.Documents.Hyperlink;
			System.Diagnostics.Process.Start((string)link.Tag);
			e.Handled = true;
		}
	}
}