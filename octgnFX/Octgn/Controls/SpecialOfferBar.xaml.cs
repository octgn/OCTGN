using System.Windows.Controls;
using System.Windows.Input;

namespace Octgn.Controls
{
    using System;
    using System.Windows;

    using Octgn.Extentions;

    /// <summary>
    /// Interaction logic for SpecialOfferBar.xaml
    /// </summary>
    public partial class SpecialOfferBar : UserControl
    {
        public SpecialOfferBar()
        {
            this.Visibility = Visibility.Collapsed;
            InitializeComponent();
            if (!this.IsInDesignMode())
            {
                SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
                var iss = SubscriptionModule.Get().IsSubscribed;
                if ((iss ?? false) == false)
                {
                    this.Visibility = Visibility.Visible;
                }
                else
                    this.Visibility = Visibility.Collapsed;
            }
        }

        private void OnIsSubbedChanged(bool b)
        {
            Dispatcher.Invoke(new Action(
                () =>
                {
                    if (b)
                    {
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.Visibility = Visibility.Visible;
                    }
                }));
        }

        private void SubscribeClick(object sender, MouseButtonEventArgs e)
        {
            var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType());
            Program.LaunchUrl(url);
        }
    }
}
