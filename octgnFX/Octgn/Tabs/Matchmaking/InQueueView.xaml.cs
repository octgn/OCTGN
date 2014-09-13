/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Reflection;
using System.Windows.Input;
using log4net;

namespace Octgn.Tabs.Matchmaking
{
    public partial class InQueueView
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public InQueueView()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
			Log.Info("Leave Queue Clicked");
            var dc = DataContext as MatchmakingTabViewModel;
            dc.LeaveQueue();
        }
    }
}
