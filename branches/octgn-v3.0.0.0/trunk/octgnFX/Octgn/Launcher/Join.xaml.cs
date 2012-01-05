using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Octgn.Networking;
using Octgn.Properties;
using System.Net.Sockets;

namespace Octgn.Launcher
{
  public sealed partial class Join : Page
  {
    private bool isStarting = false;

    public Join()
    {
      InitializeComponent();
      portBox.Text = Settings.Default.ServePort;
      nickBox.Text = Settings.Default.NickName;
      ((Storyboard)Resources["ExpandProgressBar"]).Completed += delegate { progressBar.IsIndeterminate = true; };
    }

    private delegate void NoArgsDelegate();

    private void StartClicked(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      if (isStarting) return;	// prevents doube-click and such

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

      if (gameSelector.Game == null) return;
      Program.Game = gameSelector.Game;
      if (!Program.Game.Definition.CheckVersion())
        return;

      System.Net.IPAddress ipAddress;
      if (!System.Net.IPAddress.TryParse(ipBox.Text.Trim(), out ipAddress))
      {
        MessageBox.Show("Invalid IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      int port;
      if (!int.TryParse(portBox.Text, out port))
      {
        MessageBox.Show("Invalid port number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (Program.Game != null)
      {
        // Creates a client and connect to the server
        Program.Client = new Networking.Client(ipAddress, port);
        ShowProgressBar();
        Program.Client.BeginConnect(ConnectedCallback);
        isStarting = true;
      }
    }

    private void ConnectedCallback(object sender, ConnectedEventArgs e)
    {
      isStarting = false;
      if (e.exception == null)
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgsDelegate(LaunchGame));
      else
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgsDelegate(HideProgressBar));
    }

    private void LaunchGame()
    { NavigationService.Navigate(new StartGame()); }

    private void CancelClicked(object sender, RoutedEventArgs e)
    {
      isStarting = false;
      if (startBtn.Visibility == Visibility.Visible)
        NavigationService.GoBack();
      else
      {
        Program.Client.CancelConnect();
        HideProgressBar();
      }
    }

    #region Progressbar animations

    public static readonly DependencyProperty ProgressBarAnimationProperty =
        DependencyProperty.Register("ProgressBarAnimation", typeof(double), typeof(Join), new UIPropertyMetadata(1.0, new PropertyChangedCallback(ProgressChangedCallback)));

    private static void ProgressChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
      Join wnd = (Join)source;
      wnd.spacerColumnDefinition.Width = new GridLength((double)e.NewValue, GridUnitType.Star);
    }

    private double ProgressBarAnimation
    {
      get { return (double)GetValue(ProgressBarAnimationProperty); }
      set { SetValue(ProgressBarAnimationProperty, value); }
    }

    private void ShowProgressBar()
    {
      startBtn.Visibility = Visibility.Hidden;
      progressBar.Visibility = Visibility.Visible;
      ((Storyboard)Resources["ExpandProgressBar"]).Begin(this);
    }

    private void HideProgressBar()
    {
      progressBar.Visibility = Visibility.Collapsed;
      progressBar.IsIndeterminate = false;
      startBtn.Visibility = Visibility.Visible;
      ((Storyboard)Resources["CollapseProgressBar"]).Begin(this);
    }

    #endregion
  }
}