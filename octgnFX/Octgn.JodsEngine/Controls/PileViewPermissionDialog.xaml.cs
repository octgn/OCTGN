using System.Windows;
using Octgn.Play;

namespace Octgn.Controls
{
    public partial class PileViewPermissionDialog : Window
    {
        public bool IsGranted { get; private set; }
        public bool IsPermanent { get; private set; }
        public string Message { get; private set; }

        public PileViewPermissionDialog(Group pile, Player requester)
        {
            InitializeComponent();
            
            Message = $"{requester.Name} requests permission to view {pile.FullName}.";
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlwaysRadio.IsChecked == true)
            {
                IsGranted = true;
                IsPermanent = true;
            }
            else if (YesRadio.IsChecked == true)
            {
                IsGranted = true;
                IsPermanent = false;
            }
            else if (NoRadio.IsChecked == true)
            {
                IsGranted = false;
                IsPermanent = false;
            }
            else if (NeverRadio.IsChecked == true)
            {
                IsGranted = false;
                IsPermanent = true;
            }
            
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsGranted = false;
            IsPermanent = false;
            DialogResult = false;
        }
    }
}