using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Octide.ViewModel;
using System.Windows.Input;
using System.Text;
using System;

namespace Octide.Views
{
    /// <summary>
    /// Interaction logic for ActionMenuView.xaml
    /// </summary>
    public partial class ActionMenuView : UserControl
    {
        public ActionMenuView()
        {
            InitializeComponent();
        }
        
        private void ClickAction(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModelLocator.PreviewTabViewModel.Selection = e.NewValue;
        }
    }
}