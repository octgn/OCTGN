// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Core;
using System.Windows;
using Octgn.Controls;

namespace Octgn.Play.Dialogs
{

    public partial class ChatWindow : DecorableWindow
    {

        public ChatWindow()
        {
            this.InitializeComponent();
            var size = Prefs.ChatWindowLocation;
            Left = size.Left;
            Top = size.Top;
            Height = size.Height;
            Width = size.Width;
        }



        private void Window_Closed(object sender, System.EventArgs e)
        {
            Prefs.ChatWindowLocation = new Rect(Left, Top, ActualWidth, ActualHeight);
        }

        public void FocusInput()
        {
            this.chat.FocusInput();
        }
    }



}