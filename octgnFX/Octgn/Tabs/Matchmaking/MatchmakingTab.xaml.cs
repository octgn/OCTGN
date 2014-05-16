/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;
using Octgn.Networking;
using Octgn.Play;
using Skylabs.Lobby;
using Skylabs.Lobby.Messages.Matchmaking;

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

    public class MatchmakingTabViewModel : ViewModelBase, IDisposable
    {
        private string _busyMessage;
        private bool _isBusy;
        private Game _game;
        private GameMode _mode;
        private TimeSpan _awt = TimeSpan.FromMinutes(10);
        private DateTime _startTime = DateTime.Now;
        private readonly Timer _timer;
        private readonly Timer _progressTimer;
        private Action<MatchmakingTabViewEnum> _onTransition;
        private Guid _currentQueue = Guid.Empty;
        private readonly Dispatcher _dispatcher;
        private float _readyCountdown = 100;
        private bool _showReadyDialog;
        private MatchmakingReadyRequest _currentRequest;

        public bool ShowReadyDialog
        {
            get { return _showReadyDialog; }
            set
            {
                if (_showReadyDialog == value) return;
                _showReadyDialog = value;
                RaisePropertyChanged(() => ShowReadyDialog);
            }
        }

        public float ReadyCountdown
        {
            get { return _readyCountdown; }
            set
            {
                if (_readyCountdown == value) return;
                _readyCountdown = value;
                RaisePropertyChanged(() => ReadyCountdown);
            }
        }

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
            _progressTimer = new Timer(100);
            _progressTimer.Elapsed += ProgressTimerOnElapsed;
            _progressTimer.Start();
        }

        private void ProgressTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (ReadyCountdown > 0)
            {
                ReadyCountdown -= 1f / 10f;
            }
            else if (ReadyCountdown < 0)
            {
                ReadyCountdown = 0;
            }
            if (ReadyCountdown == 0)
            {
                ShowReadyDialog = false;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            RaisePropertyChanged(() => Time);
            RaisePropertyChanged(() => AverageWaitTime);
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

        public void SignalReady()
        {
            if (_currentRequest != null)
            {
                Program.LobbyClient.Matchmaking.Ready(_currentRequest);
                Program.LobbyClient.Matchmaking.OnGameReady(OnGameReady);
            }
            this.HideReadyDialog();
        }

        public void HideReadyDialog()
        {
            this.ShowReadyDialog = false;
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

                _currentRequest = o;
                ShowReadyDialog = true;
                ReadyCountdown = 100;
                //Program.LobbyClient.Matchmaking.Ready(o);
            }
            else if (obj is MatchmakingReadyFail)
            {
                var o = obj as MatchmakingReadyFail;
                _onTransition(MatchmakingTabViewEnum.InQueue);
                HideReadyDialog();
            }
        }

        private void OnGameReady(HostedGameData obj)
        {
            var game = GameManager.Get().GetById(obj.GameGuid);
            Program.LobbyClient.CurrentHostedGamePort = (int)obj.Port;
            //Program.GameSettings.UseTwoSidedTable = true;
            Program.GameEngine = new GameEngine(game, Program.LobbyClient.Me.UserName, false, this._currentQueue.ToString().ToLower());
            Program.IsHost = false;

            var hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).First();

            // Should use gameData.IpAddress sometime.
            Program.Client = new ClientSocket(hostAddress, (int)obj.Port);
            Program.Client.Connect();
            this._dispatcher.Invoke(new Action(() =>
            {
                WindowManager.PlayWindow = new PlayWindow();
                WindowManager.PlayWindow.Show();
            }));
			this._onTransition(MatchmakingTabViewEnum.ChooseGame);
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
