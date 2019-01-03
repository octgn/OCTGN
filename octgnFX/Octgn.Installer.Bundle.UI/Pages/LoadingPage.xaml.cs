using System;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class LoadingPage : UserControl
    {
        public LoadingPage() {
            InitializeComponent();
        }
    }

    public class LoadingPageViewModel : PageViewModel
    {
        public LoadingPageViewModel() {
            Button1Visible = false;

            Page = new LoadingPage();
        }
    }
}
