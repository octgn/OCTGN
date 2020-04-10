// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Octide.ViewModel;
using System.Windows.Input;
using System.Text;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using Octgn;
using Octgn.DataNew.Entities;

namespace Octide.Views
{
    /// <summary>
    /// Interaction logic for SetTabView.xaml
    /// </summary>
    public partial class SetTabView : UserControl
    {
        public SetTabView()
        {
            InitializeComponent();
        }
    }
}