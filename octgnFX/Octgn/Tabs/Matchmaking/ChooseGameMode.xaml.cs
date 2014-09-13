/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGameMode
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ChooseGameMode()
        {
            InitializeComponent();
        }

        private void OnGameMouseUp(object sender, MouseButtonEventArgs e)
        {
			Log.Info("Clicked game mode");
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;
            var game = fe.DataContext as GameMode;
            if (game == null)
                return;
            var vm = DataContext as MatchmakingTabViewModel;
			vm.PickGameMode(game);
        }
    }
}
