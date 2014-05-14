/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGameMode
    {
        public ChooseGameMode()
        {
            InitializeComponent();
        }

        private void OnGameMouseUp(object sender, MouseButtonEventArgs e)
        {
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
