using System;
using System.Windows.Controls;

namespace Octgn.Controls
{
    using System.Globalization;
    using System.Timers;
    using System.Windows;

    /// <summary>
    /// Interaction logic for RightWebMessage.xaml
    /// </summary>
    public partial class RightWebMessage : UserControl
    {
        internal Timer UpdateProgress;
        public RightWebMessage()
        {
            InitializeComponent();
            UpdateProgress = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            UpdateProgress.Elapsed += UpdateProgressOnElapsed;
            UpdateProgress.Start();
            SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
            if (!(SubscriptionModule.Get().IsSubscribed ?? false))
            {
                SubButton.IsEnabled = true;
            }
            else
            {
                SubButton.IsEnabled = false;
            }
            Dispatcher.BeginInvoke(new Action(() => this.UpdateProgressOnElapsed(null, null)));
        }

        private void OnIsSubbedChanged(bool b)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (b)
                        SubButton.IsEnabled = false;

                    else
                    {
                        SubButton.IsEnabled = true;
                    }
                }));
        }

        private void UpdateProgressOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var c = new Site.Api.ApiClient();
            var num = c.SubPercent();
            Dispatcher.Invoke(new Action(() => this.UpdateNum(num)));
        }

        private void UpdateNum(int num)
        {
            if (num < 0)
            {
                Progress.IsIndeterminate = false;
                Progress.Value = 0;
                Progress.Maximum = 100;
                ProgressText.Text = "(Unable to Get Data)";
            }
            else
            {
                Progress.IsIndeterminate = false;
                Progress.Maximum = 100;
                Progress.Value = num;
                ProgressText.Text = num.ToString(CultureInfo.InvariantCulture) + "%";
            }
        }

        private void SubscribeClick(object sender, RoutedEventArgs e)
        {
            var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
            if (url != null)
            {
                Program.LaunchUrl(url);
            }
        }

        private void BenefitsClick(object sender, RoutedEventArgs e)
        {
            WindowManager.Main.ShowSubMessage();
        }
    }
}
