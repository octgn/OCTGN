using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Ribbon;
using Skylabs.Net;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        private bool                        _allowDirectNavigation = false;
        private NavigatingCancelEventArgs   _navArgs = null;
        private Duration                    _duration = new Duration(TimeSpan.FromSeconds(1));

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
                    case "Octgn":
                        frame1.Navigate(new MainMenu());
                        break;
                    case "!":

                        break;
                }
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);
            Program.Exit();
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            Program.LauncherWindow = new LauncherWindow();
            Program.LauncherWindow.Show();
            Program.ClientWindow.Close();
            Program.lobbyClient.Close(DisconnectReason.CleanDisconnect);            
        }
    }
}