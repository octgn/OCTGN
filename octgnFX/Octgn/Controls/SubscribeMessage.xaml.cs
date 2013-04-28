using System.Windows.Controls;

namespace Octgn.Controls
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
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


        public int TotalSeconds { get; set; }

        public SubscribeMessage()
        {
            Log.Info("Creating");
            this.Opacity = 0d;
            TotalSeconds = 60;
            this.Loaded += OnLoaded;
            this.IsVisibleChanged += OnIsVisibleChanged;

            InitializeComponent();
            SubBox.Items.Add(SubscriptionModule.Get().SubTypes);
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
                da.Completed += (o, args) => this.WaitStart();
                this.BeginAnimation(UIElement.OpacityProperty, da);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        private void WaitStart()
        {
            Log.Info("Wait started");
            var task = new Task(WaitRun);
            task.ContinueWith(WaitDone);
            task.Start();
        }

        private void WaitRun()
        {
            Log.Info("Wait running");
            var endtime = DateTime.Now.AddSeconds(TotalSeconds);
            while (DateTime.Now < endtime)
            {
                var dif = new TimeSpan(endtime.Ticks - DateTime.Now.Ticks);
                Dispatcher.Invoke(new Action(() => { this.ProgressBar.Value = dif.TotalSeconds; }));
            }
        }

        private void WaitDone(Task task)
        {
            Log.Info("Wait done");
            if (task.IsFaulted) Log.Warn("wd", task.Exception);


            Dispatcher.Invoke(
                new Action(
                    () =>
                        {
                            Log.Info("Hiding sub nag");
                            var da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(.5)));
                            da.Completed += (o, args) => this.Visibility = Visibility.Collapsed;
                            this.BeginAnimation(UIElement.OpacityProperty, da);
                        }));
        }

        private void SubscribeClick(object sender, RoutedEventArgs e)
        {
            Log.Info("Sub clicked");
            if ((SubBox.SelectedItem as SubType) == null) return;
            var subtype = (SubBox.SelectedItem as SubType);
            Log.InfoFormat("Sub clicked type {0}", subtype.Name);
            Task.Factory.StartNew(() => this.ShowSubscribeSite(subtype));
        }

        private void ShowSubscribeSite(SubType subtype)
        {
            Log.InfoFormat("Show sub site {0}",subtype);
            var url = SubscriptionModule.Get().GetSubscribeUrl(subtype);
            if (url != null)
            {
                Process.Start(url);
            }
        }
    }
}
