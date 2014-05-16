/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Input;

namespace Octgn.Tabs.Matchmaking
{
    /// <summary>
    /// Interaction logic for InQueueView.xaml
    /// </summary>
    public partial class InQueueView
    {
        public InQueueView()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            //this.Loaded -= OnLoaded;
            //var window = Window.GetWindow(this);
            //window.KeyUp += OnKeyUp;
        }

        //private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        //{
        //    Console.Beep();
        //    if (keyEventArgs.Key == Key.R)
        //    {
        //        var dx = DataContext as MatchmakingTabViewModel;
        //        dx.ReadyDialog();
        //    }
        //}
        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as MatchmakingTabViewModel;
            dc.LeaveQueue();
        }
    }
}
