using Microsoft.Windows.Controls.Ribbon;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : RibbonWindow
    {
        public Main()
        {
            InitializeComponent();
            frame1.Navigate(new MainMenu());
            // Insert code required on object creation below this point.
        }
    }
}