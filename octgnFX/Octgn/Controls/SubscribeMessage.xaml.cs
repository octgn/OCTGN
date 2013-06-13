using System.Windows.Controls;

namespace Octgn.Controls
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Animation;

    using Octgn.Library.Exceptions;

    using log4net;

    /// <summary>
    /// Interaction logic for SubscribeMessage.xaml
    /// </summary>
    public partial class SubscribeMessage : UserControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SubscribeMessage()
        {
            Log.Info("Creating");
            this.Opacity = 0d;
            this.IsVisibleChanged += OnIsVisibleChanged;

            InitializeComponent();
            Log.Info("Created");
        }

        private void OnIsVisibleChanged(
            object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Log.Info("Getting is subbed");
            var sub = SubscriptionModule.Get().IsSubscribed;
            Log.Info("Got issubbed");
            if (sub == true || sub == null) return;
            if ((bool)dependencyPropertyChangedEventArgs.NewValue)
            {
                Log.Info("Showing sub nag");
                var da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(.5)));
                da.Completed += (o, args) => this.EnableButtons();
                this.BeginAnimation(UIElement.OpacityProperty, da);
            }
        }

        private void SubscribeClick(object sender, RoutedEventArgs e)
        {
            Log.Info("Sub clicked");
            Task.Factory.StartNew(() =>
                this.ShowSubscribeSite(new SubType())).ContinueWith(x =>
                                                  {
                                                      if (x.Exception != null)
                                                      {
                                                          Log.Warn("Sub Problem", x.Exception);
                                                          TopMostMessageBox.Show(
                                                                      "Could not subscribe. Please visit "
                                                                      + AppConfig.WebsitePath + " to subscribe.",
                                                                      "Error",
                                                                      MessageBoxButton.OK,
                                                                      MessageBoxImage.Asterisk);
                                                      }
                                                  });
        }

        private void EnableButtons()
        {
            if (!Dispatcher.CheckAccess())
            {
                this.EnableButtons();
                return;
            }
            ButtonGrid.IsEnabled = true;
        }

        private void ShowSubscribeSite(SubType subtype)
        {
            Log.InfoFormat("Show sub site {0}", subtype);
            var url = SubscriptionModule.Get().GetSubscribeUrl(subtype);
            if (url != null)
            {
                Program.LaunchUrl(url);
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(.5)));
            da.Completed += (o, args) => this.Visibility = Visibility.Collapsed;
            this.BeginAnimation(UIElement.OpacityProperty, da);
        }
    }
}
