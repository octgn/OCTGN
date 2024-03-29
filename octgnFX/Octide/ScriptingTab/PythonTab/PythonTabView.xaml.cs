﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
using System.Windows.Controls;

namespace Octide.Views
{
    /// <summary>
    /// Interaction logic for PythonTabView.xaml
    /// </summary>
    public partial class PythonTabView : UserControl
    {
        public PythonTabView()
        {
            InitializeComponent();
            textEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
        }
    }
}