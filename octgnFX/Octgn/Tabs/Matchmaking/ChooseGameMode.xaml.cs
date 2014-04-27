using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGameMode
    {
        public ObservableCollection<GameMode> GameModes { get; set; }

        public Game SelectedGame
        {
            get
            {
                return _game;
            }
            set
            {
                if (_game == value) return;
                _game = value;
				RefreshModes();
            }
        }

        public event Action<GameMode> OnChooseGameMode;

        private Game _game;

        public ChooseGameMode()
        {
			GameModes = new ObservableCollection<GameMode>();
            InitializeComponent();
        }

        private void RefreshModes()
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(RefreshModes));
                return;
            }
            GameModes.Clear();

            if (SelectedGame == null)
                return;

            //pack://application:,,,/OCTGN;component/Resources/gamemode.png"
            foreach (var m in SelectedGame.Modes)
            {
                if (m.Image == null)
                    m.Image = "pack://application:,,,/OCTGN;component/Resources/gamemode.png";
                GameModes.Add(m);
            }
        }

        private void OnGameMouseUp(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;
            var game = fe.DataContext as GameMode;
            if (game == null)
                return;
            if (OnChooseGameMode != null)
                OnChooseGameMode(game);
        }
    }
}
