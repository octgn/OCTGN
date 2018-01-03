using System.Windows.Controls;
using Octide.ViewModel;

namespace Octide.Views
{
    using System.Windows;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;

    public partial class TableView : UserControl
    {
        public TableView()
        {
            InitializeComponent();
        }
    }
}