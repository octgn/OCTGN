// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Core;
using Octgn.Play.Gui;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octgn.Play.Dialogs
{

    public partial class PreviewCardWindow : Window
    {
        public PreviewCardWindow()
        {
            this.InitializeComponent();
            var size = Prefs.PreviewCardWindowLocation;
            Left = size.Left;
            Top = size.Top;
            Height = size.Height;
            Width = size.Width;
        }

        public void DisplayImage(BitmapSource image)
        {
            cardViewer.Source = image;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Prefs.PreviewCardWindowLocation = new Rect(Left, Top, ActualWidth, ActualHeight);
        }
    }
}