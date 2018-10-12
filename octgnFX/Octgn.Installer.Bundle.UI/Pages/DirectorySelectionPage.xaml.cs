using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    /// <summary>
    /// Interaction logic for DirectorySelectionPage.xaml
    /// </summary>
    public partial class DirectorySelectionPage : UserControl
    {
        public DirectorySelectionPage() {
            InitializeComponent();
        }
    }

    public class DirectorySelectionPageViewModel : PageViewModel
    {
        public DirectorySelectionPageViewModel() {
            Button1Text = "Install";

            Page = new DirectorySelectionPage() {
                DataContext = this
            };
        }
    }
}
