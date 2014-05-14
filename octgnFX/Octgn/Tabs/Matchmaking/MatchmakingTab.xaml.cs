/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Skylabs.Lobby.Messages.Matchmaking;

namespace Octgn.Tabs.Matchmaking
{
    public partial class MatchmakingTab
    {
        public MatchmakingTabViewModel VM { get; set; }
        public MatchmakingTab()
        {
            VM = new MatchmakingTabViewModel();
            VM.SetStartOver(OnStartOver);
            InitializeComponent();
        }

        private void ChooseGame_OnChooseGame(Game obj)
        {
            VM.Game = obj;
            ChooseGameModeView.SelectedGame = obj;
            ChooseGameModeView.IsHitTestVisible = true;
            ChooseGameView.IsHitTestVisible = false;
            var sl = this.FindResource("SlideLeftHide") as Storyboard;
            Storyboard.SetTarget(sl, ChooseGameView);
            sl.Begin();

            sl = this.FindResource("SlideLeftShow") as Storyboard;
            Storyboard.SetTarget(sl, ChooseGameModeView);
            sl.Begin();
        }

        private void ChooseGame_OnChooseGameMode(GameMode obj)
        {
            VM.Mode = obj;
            ChooseGameView.IsHitTestVisible = false;
            ChooseGameModeView.IsHitTestVisible = false;

            var sl = this.FindResource("SlideLeftHide") as Storyboard;
            Storyboard.SetTarget(sl, ChooseGameModeView);
            sl.Begin();
            VM.DoStartMatchmaking();
        }

        private void OnStartOver()
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(OnStartOver));
                return;
            }
            ChooseGameView.IsHitTestVisible = true;
            ChooseGameModeView.IsHitTestVisible = false;

            var sl = this.FindResource("SlideLeftShow") as Storyboard;
            Storyboard.SetTarget(sl, ChooseGameView);
            sl.Begin();
        }
    }

    public class MatchmakingTabViewModel : ViewModelBase
    {
        private string _busyMessage;
        private bool _isBusy;
        private Game _game;
        private GameMode _mode;
        private Action _onStartOverAction;

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
            }
        }

        public MatchmakingTabViewModel()
        {
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
                _onStartOverAction();
                IsBusy = false;
                BusyMessage = "";
                return;
            }

            //TODO at this point we move to a queue screen.
            _onStartOverAction();
            IsBusy = false;
            BusyMessage = "";
        }

        public void SetStartOver(Action onStartOver)
        {
            _onStartOverAction = onStartOver;
        }
    }
}
