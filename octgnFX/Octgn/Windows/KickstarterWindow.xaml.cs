using System;

namespace Octgn.Windows
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for KickstarterWindow.xaml
    /// </summary>
    public partial class KickstarterWindow
    {
        public KickstarterWindow()
        {
            //this.Loaded += (sender, args) => 
            InitializeComponent();
            browser.Loaded += BrowserOnLoaded;
        }

        private void BrowserOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.browser.Navigate("http://www.indiegogo.com/project/394000/widget/1640515");
        }
    }
}
