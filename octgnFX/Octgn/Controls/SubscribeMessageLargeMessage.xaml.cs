using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Octgn.Extentions;
using System.Timers;
using System.Globalization;
using System.Windows;

namespace Octgn.Controls
{
    public partial class SubscribeMessageLarge : INotifyPropertyChanged
    {
        internal Timer UpdateProgress;

        public int PercentNum
        {
            get { return _percentNum; }
            set
            {
                if (value == _percentNum) return;
                _percentNum = value;
                OnPropertyChanged("PercentNum");
            }
        }

        public SubscribeMessageLarge()
        {
            InitializeComponent();
            UpdateProgress = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            UpdateProgress.Elapsed += UpdateProgressOnElapsed;
            UpdateProgress.Start();
            this.UpdateProgressOnElapsed(null, null);
        }

        private int _percentNum;

        private void UpdateProgressOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Dispatcher.CheckAccess())
            {
                Task.Factory.StartNew(() => UpdateProgressOnElapsed(sender, elapsedEventArgs));
                return;
            }
            UpdateProgress.Enabled = false;
            try
            {
                var c = new Site.Api.ApiClient();
                var num = c.SubPercent();
                Dispatcher.Invoke(new Action(() => this.UpdateNum(num)));

            }
            finally
            {
                UpdateProgress.Enabled = true;
            }
        }

        private void UpdateNum(int num)
        {
            if (num < 0)
            {
                Progress.IsIndeterminate = false;
                Progress.Value = 0;
                Progress.Maximum = 100;
                ProgressText.Text = "(Unable to Get Data)";
                PercentNum = 0;
            }
            else
            {
                Progress.IsIndeterminate = false;
                Progress.Maximum = 100;
                Progress.Value = num;
                PercentNum = num;
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


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
