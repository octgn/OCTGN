/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace Octgn.Tabs.Matchmaking
{
    public partial class MatchmakingTab
    {
        public MatchmakingTabViewModel VM { get; set; }
        private readonly Dictionary<MatchmakingTabViewEnum, FrameworkElement> _pages = new Dictionary<MatchmakingTabViewEnum, FrameworkElement>();
        private MatchmakingTabViewEnum _currentPage = MatchmakingTabViewEnum.ChooseGame;
        public MatchmakingTab()
        {
            VM = new MatchmakingTabViewModel(Dispatcher);
            VM.SetTransition(OnTransition);

            InitializeComponent();

            _pages.Add(MatchmakingTabViewEnum.ChooseGame, ChooseGameView);
            _pages.Add(MatchmakingTabViewEnum.ChooseMode, ChooseGameModeView);
            _pages.Add(MatchmakingTabViewEnum.InQueue, InQueueView);
        }

        private void OnTransition(MatchmakingTabViewEnum obj)
        {
            if (_pages.ContainsKey(obj) == false)
                throw new InvalidOperationException("Page " + obj.ToString() + " doesn't exist");

            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(() => OnTransition(obj)));
                return;
            }

            var page = _pages[obj];
            var curPage = _pages[_currentPage];

            foreach (var p in _pages)
            {
                p.Value.IsHitTestVisible = false;
            }

            page.IsHitTestVisible = true;

            var sl = this.FindResource("SlideLeftHide") as Storyboard;
            Storyboard.SetTarget(sl, curPage);
            sl.Begin();

            var sl2 = this.FindResource("SlideLeftShow") as Storyboard;
            Storyboard.SetTarget(sl2, page);
            sl2.Begin();
            _currentPage = obj;
        }
    }

    public enum MatchmakingTabViewEnum
    {
        ChooseGame, ChooseMode, InQueue
    }
}
