// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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