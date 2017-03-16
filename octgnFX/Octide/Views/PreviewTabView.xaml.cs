using System.Windows.Controls;
using Octide.ViewModel;

namespace Octide.Views
{
    using System.Windows;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using System.Windows.Data;
    using Octgn.Play;

    public partial class PreviewTabView : UserControl
    {
        public PreviewTabView()
        {
            InitializeComponent();
        }

        
        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}