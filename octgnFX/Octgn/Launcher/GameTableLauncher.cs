namespace Octgn.Launcher
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.Library.Exceptions;

    using Skylabs.Lobby;

    using log4net;

    public class GameTableLauncher
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Launch()
        {
            var gameId = Prefs.LastHostedGameType;
            var game = GameManager.Get().GetById(gameId);
            Program.Dispatcher = Application.Current.Dispatcher;
            StartLocalGame(game, Skylabs.Lobby.Randomness.RandomRoomName(), null);
            Program.GameSettings.UseTwoSidedTable = Prefs.TwoSidedTable;
            if (Program.GameEngine != null)
                Dispatcher.CurrentDispatcher.Invoke(new Action(Program.GameEngine.Begin));
            Program.StartGame();
            Application.Current.MainWindow = WindowManager.PlayWindow;
            WindowManager.PlayWindow.Closed += PlayWindowOnClosed;
        }

        private void PlayWindowOnClosed(object sender, EventArgs eventArgs)
        {
            Program.Exit();
        }

        void StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var hostport = new Random().Next(5000, 6000);
            while (!Networking.IsPortAvailable(hostport)) hostport++;
            var hs = new HostedGame(hostport, game.Id, game.Version, game.Name, name, null, new User(Prefs.Nickname + "@" + AppConfig.ChatServerPath), true);
            if (!hs.StartProcess())
            {
                throw new UserMessageException("Cannot start local game. You may be missing a file.");
            }
            Program.LobbyClient.CurrentHostedGamePort = hostport;
            Program.GameSettings.UseTwoSidedTable = true;
            Program.GameEngine = new GameEngine(game, Prefs.Nickname, true);
            Program.IsHost = true;

            var ip = IPAddress.Parse("127.0.0.1");

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Program.Client = new Octgn.Networking.Client(ip, hostport);
                    Program.Client.Connect();
                    return;
                }
                catch (Exception e)
                {
                    Log.Warn("Start local game error", e);
                    if (i == 4) throw;
                }
                Thread.Sleep(2000);
            }
            throw new UserMessageException("Cannot start local game. You may be missing a file.");
        }
    }
}