using Octgn.DataNew;
using Octgn.Tabs.GameHistory;
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace Octgn.Windows
{
    public partial class GameHistoryWindow
    {
        public GameHistoryViewModel History { get; }

        public string GameImage {
            get => _gameImage;
            set {
                if (value != _gameImage) {
                    _gameImage = value;
                    NotifyPropertyChanged(nameof(GameImage));
                }
            }
        }
        private string _gameImage;

        public string GameName {
            get => _gameName;
            set {
                if(value != _gameName) {
                    _gameName = value;
                    NotifyPropertyChanged(nameof(GameName));
                }
            }
        }
        private string _gameName;

        public GameHistoryWindow()
        {
            InitializeComponent();
        }

        public GameHistoryWindow(GameHistoryViewModel history) {
            History = history ?? throw new ArgumentNullException(nameof(history));

            InitializeComponent();

            DataContext = this;

            Loaded += GameHistoryWindow_Loaded;
        }

        private void GameHistoryWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            Loaded -= GameHistoryWindow_Loaded;

            var game = DbContext.Get().GameById(History.GameId);

            if (game != null) {
                GameImage = game.IconUrl;
                GameName = game.Name;
            } else {
                GameImage = "pack://application:,,,/Octgn;Component/Resources/FileIcons/Game.ico";
                GameName = "UNKNOWN GAME";
            }

            ChatLogTextBox.Document.Blocks.Clear();

            if (string.IsNullOrWhiteSpace(History.LogFile) || !File.Exists(History.LogFile)) {
                ChatLogTextBox.Document.Blocks.Add(new Paragraph(new Run("No log file found.")));
            } else {
                var logString = File.ReadAllLines(History.LogFile);

                var pg = new Paragraph();
                foreach(var line in logString) {
                    pg.Inlines.Add(new Run(line + Environment.NewLine));
                }
                ChatLogTextBox.Document.Blocks.Add(pg);
            }
        }

        private void Replay_Click(object sender, System.Windows.RoutedEventArgs e) {
            if((SubscriptionModule.Get().IsSubscribed ?? false) == false) {
                MessageBox.Show("Replays are currently only available for subscribers.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            var game = DbContext.Get().GameById(History.GameId);

            if(game == null) {
                MessageBox.Show("Unable to watch replay, the game isn't installed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            Program.JodsEngine.LaunchReplay(History);

            //var replayClient = new ReplayClient();
            //Program.Client = replayClient;

            //// TODO: Should set username to the user that created the replay

            //Program.IsHost = true;
            //Program.CurrentOnlineGameName = History.GameName;

            //ReplayReader reader = null;
            //ReplayEngine engine = null;
            //try {
            //    reader = ReplayReader.FromStream(File.OpenRead(History.ReplayFile));
            //    engine = new ReplayEngine(reader, replayClient);

            //    Program.GameEngine = new GameEngine(engine, game, reader.Replay.User);
            //} catch {
            //    reader?.Dispose();
            //    engine?.Dispose();

            //    throw;
            //}

            //Program.IsHost = true;
            //LaunchPlayWindow();
        }
    }
}
