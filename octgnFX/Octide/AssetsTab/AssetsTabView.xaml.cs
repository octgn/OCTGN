// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Controls;

namespace Octide.Views
{
    using System;
    using System.IO;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml;

    using Octide.ViewModel;

    /// <summary>
    /// Interaction logic for AssetView.xaml
    /// </summary>
    public partial class AssetsTabView : UserControl
    {
        public AssetsTabView()
        {
            InitializeComponent();
        }
    }
}
