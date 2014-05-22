/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using log4net;
using Octgn.Controls;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;
using Octgn.Networking;
using Octgn.Play;
using Skylabs.Lobby;
using Skylabs.Lobby.Messages.Matchmaking;

namespace Octgn.Tabs.Matchmaking
{
    public class MatchmakingTabViewModel : ViewModelBase, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        private DateTime _lastMatchmakingMessage = DateTime.Now;

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

        public List<GameType> GameTypeList { get; set; }

        public MatchmakingTabViewModel(Dispatcher dispatcher)
        {
            Log.Info("Creating matchmaking view.");
            GameTypeList = new List<GameType>();
            var g1 = new GameType();
            var g2 = new GameType();
            var g3 = new GameType();
            g1.Name = "Matchmaking";
            g1.Icon = "pack://application:,,,/OCTGN;component/Resources/matchmaking-icon.png";
            g2.Name = "Custom Games";
            g2.Icon = "pack://application:,,,/OCTGN;component/Resources/custom-games-icon.png";
            g3.Name = "Spectating";
            g3.Icon = "pack://application:,,,/OCTGN;component/Resources/spectator-icon.png";

            GameTypeList.Add(g1);
            GameTypeList.Add(g2);
            GameTypeList.Add(g3);
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
            if (ReadyCountdown == 0 && ShowReadyDialog)
            {
                ShowReadyDialog = false;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            RaisePropertyChanged(() => Time);
            RaisePropertyChanged(() => AverageWaitTime);
            if (_currentQueue != Guid.Empty)
            {
                if (new TimeSpan(DateTime.Now.Ticks - _lastMatchmakingMessage.Ticks).TotalSeconds > 120)
                {
                    // Haven't heard from the server in waaaay to long. Leave matchmaking.
                    Log.Warn("Haven't heard from the matchmaking server in 2 minutes. Leaving queue.");
                    ResetToBeginning();
                }
            }
        }

        public void PickGame(Game g)
        {
            if (g == null)
            {
				Log.Warn("Tried to pick null game?");
                return;
            }
            Log.InfoFormat("Picking game {0}:{1}",g.Id,g.Name);
            Game = g;
            _onTransition(MatchmakingTabViewEnum.ChooseMode);
        }

        public void PickGameMode(GameMode g)
        {
            if (g == null)
            {
				Log.Warn("Tried to pick null game mode?");
                return;
            }
            if (g.Name == "Back")
            {
                Log.Info("Going back to choose game");
				ResetToBeginning();
            }
            else
            {
				Log.InfoFormat("Picking game mode {0}",g.Name);
				Mode = g;
				DoStartMatchmaking();
            }
        }

        public void PickGameType(GameType g)
        {
            if (g == null)
            {
                Log.Warn("Tried to pick null game type?");
                return;
            }
        }

        public void DoStartMatchmaking()
        {
			Log.Info("Starting matchmaking...");
            IsBusy = true;
            BusyMessage = "Starting Matchmaking...";

            Program.LobbyClient.Matchmaking.JoinMatchmakingQueueAsync(Game, Mode, 10000)
                .ContinueWith(FinishedMatchamkingRequest);
        }

        public void SignalReady()
        {
            if (_currentRequest != null)
            {
                Log.Info("Sending ready message");
                Program.LobbyClient.Matchmaking.Ready(_currentRequest);
                Program.LobbyClient.Matchmaking.OnGameReady(OnGameReady);
            }
            else
            {
                Log.Warn("Tried to signal ready, but we were not in a queue");
            }
            this.HideReadyDialog();
        }

        public void HideReadyDialog()
        {
			Log.Info("Hiding ready dialog");
            this.ShowReadyDialog = false;
        }

        private void FinishedMatchamkingRequest(Task<StartMatchmakingResponse> obj)
        {
            if (obj.IsFaulted || obj.IsCanceled || obj.Result == null)
            {
                Log.Warn("FinishedMatchamkingRequest Failed",obj.Exception);
                WindowManager.Main.Dispatcher.Invoke(new Action(() =>
                {
					Log.Info("Displaying message box");
                    MessageBox.Show(WindowManager.Main, "Could not start matchmaking. Please try again.", "Matchmaking Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }));
                ResetToBeginning();
                return;
            }

			Log.Info("Finished matchmaking request");
            _lastMatchmakingMessage = DateTime.Now;
            _currentQueue = obj.Result.QueueId;
            Program.LobbyClient.Matchmaking.OnMatchmakingUpdate(OnMatchmakingUpdate);
            _onTransition(MatchmakingTabViewEnum.InQueue);
            IsBusy = false;
            BusyMessage = "";
            _startTime = DateTime.Now;
        }

        private void OnMatchmakingUpdate(MatchmakingMessage obj)
        {
            if (obj == null)
            {
                Log.Warn("OnMatchmakingUpdate obj is null?");
                return;
            }
			Log.InfoFormat("OnMatchmakingUpdate {0}",obj.GetType().Name);
            _lastMatchmakingMessage = DateTime.Now;
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
            try
            {
				Log.InfoFormat("Game is ready {0}",obj);
                _lastMatchmakingMessage = DateTime.Now;
				Log.Info("Getting game...");
                var game = GameManager.Get().GetById(obj.GameGuid);
                Program.LobbyClient.CurrentHostedGamePort = (int)obj.Port;
                //Program.GameSettings.UseTwoSidedTable = true;
				Log.Info("Creating game engine");
                Program.GameEngine = new GameEngine(game, Program.LobbyClient.Me.UserName, false, this._currentQueue.ToString().ToLower());
                Program.IsHost = false;
                Program.IsMatchmaking = true;
                Program.GameMode = this._mode;

                var hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).First();

                // Should use gameData.IpAddress sometime.
                Program.Client = new ClientSocket(hostAddress, (int)obj.Port);
				Log.Info("Connecting...");
                Program.Client.Connect();
                this._dispatcher.Invoke(new Action(() =>
                {
					Log.Info("Launching play window");
                    WindowManager.PlayWindow = new PlayWindow();
                    WindowManager.PlayWindow.Show();
					Log.Info("Finished launching play window");
                }));

            }
            catch (Exception e)
            {
                Log.Warn("Error joining matchmaking game", e);
                TopMostMessageBox.Show("Can't join matchmaking game, there is a problem with your connection.", "Error",MessageBoxButton.OK, MessageBoxImage.Error);
            } 
            ResetToBeginning();
        }

        private void RefreshModes()
        {
            if (_dispatcher.CheckAccess() == false)
            {
                _dispatcher.Invoke(new Action(RefreshModes));
                return;
            }
			Log.Info("Refreshing modes");
            GameModes.Clear();

            if (Game == null)
            {
				Log.Warn("Game is null, can't refresh modes. User probably uninstalled the game.");
                return;
            }
            //pack://application:,,,/OCTGN;component/Resources/gamemode.png"
            var mode = new GameMode();
            mode.Name = "Back";
            mode.Image = "pack://application:,,,/OCTGN;component/Resources/circle-back-button.png";
			GameModes.Add(mode);
            foreach (var m in Game.Modes)
            {
                if (m.Image == null)
                    m.Image = "pack://application:,,,/OCTGN;component/Resources/gamemode.png";
                GameModes.Add(m);
            }
			Log.Info("Refreshed modes");
        }

        private void ResetToBeginning()
        {
			Log.Info("Resseting To the beginning");
            Game = null;
            Mode = null;
            _onTransition(MatchmakingTabViewEnum.ChooseGame);
            IsBusy = false;
            BusyMessage = "";
            _currentQueue = Guid.Empty;
        }

        public void LeaveQueue()
        {
			Log.Info("Leaving queue");
            Program.LobbyClient.Matchmaking.LeaveQueue(_currentQueue);
            ResetToBeginning();
        }

        public void SetTransition(Action<MatchmakingTabViewEnum> page)
        {
			Log.Info("Setting transition action");
            _onTransition = page;
        }

        public void Dispose()
        {
			Log.Info("Disposing");
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Dispose();
        }
    }
    public class GameType
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}