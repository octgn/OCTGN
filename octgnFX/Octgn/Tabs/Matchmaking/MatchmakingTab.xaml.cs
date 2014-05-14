/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Skylabs.Lobby.Messages.Matchmaking;

namespace Octgn.Tabs.Matchmaking
{
    public partial class MatchmakingTab
    {
        public MatchmakingTabViewModel VM { get; set; }
		private readonly Dictionary<MatchmakingTabViewEnum,FrameworkElement> _pages = new Dictionary<MatchmakingTabViewEnum, FrameworkElement>();
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
            if(_pages.ContainsKey(obj) == false)
				throw new InvalidOperationException("Page " + obj.ToString() + " doesn't exist");

            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(()=>OnTransition(obj)));
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
        ChooseGame,ChooseMode,InQueue
    }

    public class MatchmakingTabViewModel : ViewModelBase, IDisposable
    {
        private string _busyMessage;
        private bool _isBusy;
        private Game _game;
        private GameMode _mode;
        private TimeSpan _awt = TimeSpan.FromMinutes(10);
        private DateTime _startTime = DateTime.Now;
        private readonly Timer _timer;
        private Action<MatchmakingTabViewEnum> _onTransition;
        private Guid _currentQueue = Guid.Empty;
        private Dispatcher _dispatcher;

        public string AverageWaitTime
        {
            get
            {
                return _awt.ToString(@"mm\:ss");
            }
        }

        public string Time
        {
            get
            {
                var ts = new TimeSpan(DateTime.Now.Ticks - _startTime.Ticks);
                return ts.ToString(@"mm\:ss");
            }
        }

        public GameMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;
                _mode = value;
                RaisePropertyChanged(() => Mode);
            }
        }

        public string BusyMessage
        {
            get { return _busyMessage; }
            set
            {
                if (value == _busyMessage)
                    return;
                _busyMessage = value;
                this.RaisePropertyChanged("BusyMessage");
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy)
                    return;
                _isBusy = value;
                this.RaisePropertyChanged("IsBusy");
            }
        }

        public Game Game
        {
            get { return _game; }
            set
            {
                if (_game == value) return;
                _game = value;
                RaisePropertyChanged(() => Game);
				RefreshModes();
            }
        }

        public ObservableCollection<GameMode> GameModes { get; set; }

        public MatchmakingTabViewModel(Dispatcher dispatcher)
        {
			GameModes = new ObservableCollection<GameMode>();
            _dispatcher = dispatcher;
            _timer = new Timer(1000);
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            RaisePropertyChanged(()=>Time);
			RaisePropertyChanged(()=>AverageWaitTime);
        }

        public void PickGame(Game g)
        {
            Game = g;
			_onTransition(MatchmakingTabViewEnum.ChooseMode);
        }

        public void PickGameMode(GameMode g)
        {
            Mode = g;
			DoStartMatchmaking();
        }

        public void DoStartMatchmaking()
        {
            IsBusy = true;
            BusyMessage = "Starting Matchmaking...";

            Program.LobbyClient.Matchmaking.JoinMatchmakingQueueAsync(Game, Mode, 10000)
                .ContinueWith(FinishedMatchamkingRequest);
        }

        private void FinishedMatchamkingRequest(Task<StartMatchmakingResponse> obj)
        {
            if (obj.IsFaulted || obj.IsCanceled || obj.Result == null)
            {
                WindowManager.Main.Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show(WindowManager.Main, "Could not start matchmaking. Please try again.", "Matchmaking Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }));
                Game = null;
                Mode = null;
                _onTransition(MatchmakingTabViewEnum.ChooseGame);
                IsBusy = false;
                BusyMessage = "";
                return;
            }

            _currentQueue = obj.Result.QueueId;
			Program.LobbyClient.Matchmaking.OnMatchmakingUpdate(OnMatchmakingUpdate);
			_onTransition(MatchmakingTabViewEnum.InQueue);
            IsBusy = false;
            BusyMessage = "";
            _startTime = DateTime.Now;
        }

        private void OnMatchmakingUpdate(MatchmakingMessage obj)
        {
            if (obj is MatchmakingInLineUpdateMessage)
            {
                var o = obj as MatchmakingInLineUpdateMessage;
                _awt = o.AverageWaitTime;
            }
			else if (obj is MatchmakingReadyRequest)
			{
			    var o = obj as MatchmakingReadyRequest;
				//TODO add some sort of UI so the user has to actually click ready
				Program.LobbyClient.Matchmaking.Ready(o);
				//TODO Show an update to the waiting screen saying something like "Waiting for game" or something
			}
        }

        private void RefreshModes()
        {
            if (_dispatcher.CheckAccess() == false)
            {
                _dispatcher.Invoke(new Action(RefreshModes));
                return;
            }
            GameModes.Clear();

            if (Game == null)
                return;

            //pack://application:,,,/OCTGN;component/Resources/gamemode.png"
            foreach (var m in Game.Modes)
            {
                if (m.Image == null)
                    m.Image = "pack://application:,,,/OCTGN;component/Resources/gamemode.png";
                GameModes.Add(m);
            }
        }

        public void LeaveQueue()
        {
            Program.LobbyClient.Matchmaking.LeaveQueue(_currentQueue);
            IsBusy = false;
            BusyMessage = "";

			_onTransition(MatchmakingTabViewEnum.ChooseGame);
        }

        public void SetTransition(Action<MatchmakingTabViewEnum> page)
        {
            _onTransition = page;
        }

        public void Dispose()
        {
            _timer.Elapsed -= TimerOnElapsed;
			_timer.Dispose();
        }
    }
}
