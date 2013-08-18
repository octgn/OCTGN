using System.Windows.Controls;

namespace Octgn.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Animation;

    using Octgn.Extentions;

    using log4net;

    /// <summary>
    /// Interaction logic for SubscribeMessage.xaml
    /// </summary>
    public partial class SubscribeMessage : UserControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string BenifitList { get; private set; }

        public SubscribeMessage()
        {
            Log.Info("Creating");
            BenifitList = "Could not load benefit list";
            if (!this.IsInDesignMode())
            {
                this.Opacity = 0d;
                this.IsVisibleChanged += OnIsVisibleChanged;
                var list = SubscriptionModule.Get().Benefits.Select(x => "* " + x);
                BenifitList = String.Join(Environment.NewLine, list);
            }

            InitializeComponent();
            Log.Info("Created");
        }

        private void OnIsVisibleChanged(
            object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            //Log.Info("Getting is subbed");
            //var sub = SubscriptionModule.Get().IsSubscribed;
            //Log.Info("Got issubbed");
            //if (sub == true || sub == null) return;
            if ((bool)dependencyPropertyChangedEventArgs.NewValue)
            {
                Log.Info("Showing sub nag");
                var da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(2)));
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
                                                      this.Close();
                                                  });
        }

        private void EnableButtons()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.EnableButtons));
                return;
            }
            ButtonGrid.IsEnabled = true;
        }
        private void DisableButtons()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.DisableButtons));
                return;
            }
            ButtonGrid.IsEnabled = false;
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
            this.Close();
        }

        private void Close()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(this.Close));
                return;
            }
            this.DisableButtons();
            var da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(2)));
            da.Completed += (o, args) => this.Visibility = Visibility.Collapsed;
            this.BeginAnimation(UIElement.OpacityProperty, da);
        }
    }
}
