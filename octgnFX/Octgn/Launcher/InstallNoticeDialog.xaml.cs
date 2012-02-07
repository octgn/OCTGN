using System.Windows;
using Octgn.Properties;

namespace Octgn.Launcher
{
    public partial class InstallNoticeDialog : Window
    {
        public static readonly DependencyProperty DontShowAgainProperty =
            DependencyProperty.Register("DontShowAgain", typeof (bool), typeof (InstallNoticeDialog),
                                        new UIPropertyMetadata(false));

        public InstallNoticeDialog()
        {
            InitializeComponent();
        }

        public bool DontShowAgain
        {
            get { return (bool) GetValue(DontShowAgainProperty); }
            set { SetValue(DontShowAgainProperty, value); }
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            if (DontShowAgain)
            {
                Settings.Default.DontShowInstallNotice = true;
                Settings.Default.Save();
            }
            Close();
        }
    }
}