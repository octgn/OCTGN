using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using log4net;
using Octgn.DataNew.Entities;
using Octgn.Core.DataManagers;
using Octgn.Core;
using Octgn.Library.Exceptions;
using Octgn.Play;
using Skylabs.Lobby;

namespace Octgn.Launchers
{

    public class GameTableLauncher
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal int HostPort;
        internal Game HostGame;
        internal string HostUrl;

        public void Launch(int? hostport, Guid? game)
        {
            Program.Dispatcher = Application.Current.Dispatcher;
            HostGame = GameManager.Get().GetById(game.Value);
            if (hostport == null || hostport <= 0)
            {
                this.HostPort = new Random().Next(5000, 6000);
                while (!Skylabs.Lobby.Networking.IsPortAvailable(this.HostPort)) this.HostPort++;
            }
            else
            {
                this.HostPort = hostport.Value;
            }
            // Host a game
            this.Host();
        }

        private void Host()
        {
            StartLocalGame(HostGame, Skylabs.Lobby.Randomness.RandomRoomName(), "");
            Octgn.Play.Player.OnLocalPlayerWelcomed += PlayerOnOnLocalPlayerWelcomed;
            Program.GameSettings.UseTwoSidedTable = HostGame.UseTwoSidedTable;
            if (Program.GameEngine != null)
                Dispatcher.CurrentDispatcher.Invoke(new Action(()=>Program.GameEngine.Begin()));
        }

        private void PlayerOnOnLocalPlayerWelcomed()
        {
            if (Octgn.Play.Player.LocalPlayer.Id == 1)
            {
                this.StartGame();
            }
        }

        private void StartGame()
        {
            Play.Player.OnLocalPlayerWelcomed -= this.PlayerOnOnLocalPlayerWelcomed;
            WindowManager.PlayWindow = new PlayWindow();
            Application.Current.MainWindow = WindowManager.PlayWindow;
			//WindowManager.PlayWindow.PreGameLobby.Start(false);
			WindowManager.PlayWindow.Show();
            WindowManager.PlayWindow.Closed += PlayWindowOnClosed;
        }

        private void PlayWindowOnClosed(object sender, EventArgs eventArgs)
        {
            Program.Exit();
        }

        void StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var hs = new HostedGame(HostPort, game.Id, game.Version, game.Name,game.IconUrl, name, null, new Skylabs.Lobby.User(Prefs.Nickname + "@" + AppConfig.ChatServerPath),true, true);
            if (!hs.StartProcess())
            {
                throw new UserMessageException("Cannot start local game. You may be missing a file.");
            }
            Program.LobbyClient.CurrentHostedGamePort = HostPort;
            Program.GameSettings.UseTwoSidedTable = HostGame.UseTwoSidedTable;
            Program.IsHost = true;
            Program.IsMatchmaking = false;
            Program.GameEngine = new GameEngine(game, Prefs.Nickname, false,password,true);

            var ip = IPAddress.Parse("127.0.0.1");

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Program.Client = new Octgn.Networking.ClientSocket(ip, HostPort);
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