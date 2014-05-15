/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as MatchmakingTabViewModel;
            dc.LeaveQueue();
        }
    }
}
