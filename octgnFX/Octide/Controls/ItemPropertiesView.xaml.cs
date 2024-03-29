﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Controls;

namespace Octide.Controls
{
    /// <summary>
    /// Interaction logic for ItemPropertiesView.xaml
    /// </summary>
    public partial class ItemPropertiesView : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string),
                typeof(ItemPropertiesView), new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string),
                typeof(ItemPropertiesView), new PropertyMetadata(null));

        public ItemPropertiesView()
        {
            InitializeComponent();
        }


    }
}
