using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using log4net;
using Octgn.Library;
using Octgn.Library.Networking;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;
using Skylabs.Lobby;
using HostedGame = Skylabs.Lobby.HostedGame;

namespace Octgn.Online.GameService
{
    public class GameManager : IDisposable
    {
        #region Singleton

        internal static GameManager SingletonContext { get; set; }

        private static readonly object GameManagerSingletonLocker = new object();

        public static GameManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal GameBroadcastListener GameListener;
        internal Timer UpdateWebsiteTimer;

        public void Start()
        {
            GameListener = new GameBroadcastListener(AppConfig.Instance.BroadcastPort);
            GameListener.StartListening();
			UpdateWebsiteTimer = new Timer(30000);
			UpdateWebsiteTimer.Elapsed += UpdateWebsiteTimerOnElapsed;
			if(AppConfig.Instance.TestMode == false)
				UpdateWebsiteTimer.Start();
        }

        public IEnumerable<HostedGameData> Games
        {
            get
            {
                return GameListener.Games
                    .Select(x => new HostedGameData(x.Id,x.GameGuid,x.GameVersion,x.Port
                        ,x.Name,new User(x.Username + "@of.octgn.net"),x.TimeStarted,x.GameName,
							x.GameIconUrl,x.HasPassword,Ports.ExternalIp,x.Source ,x.GameStatus,x.Spectator))
                    .ToArray();
            }
        }

        public Guid HostGame(HostGameRequest req, User u)
        {
            var bport = AppConfig.Instance.BroadcastPort;

            var game = new HostedGame(Ports.NextPort, req.GameGuid, req.GameVersion,
                req.GameName,req.GameIconUrl, req.Name, req.Password, u,req.Spectators ,false, true
                ,req.RequestId,bport,req.SasVersion);

            if (game.StartProcess(true))
            {
                // Try to kill every other game this asshole started before this one.
                var others = GameListener.Games.Where(x => x.Username.Equals(u.UserName, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                foreach (var g in others)
                {
                    g.TryKillGame();
                }
                return game.Id;
            }
            return Guid.Empty;
        }

        public void KillGame(Guid id)
        {
            var g = GameListener.Games.FirstOrDefault(x => x.Id == id);
            if(g == null)
                throw new Exception("Game with id " + id + " can't be found.");

            var p = Process.GetProcessById(g.ProcessId);
            if(p == null)
                throw new Exception("Can't find process with id " + g.ProcessId);

            X.Instance.Try(p.Kill);
            
        }

        private bool ticking = false;
        private void UpdateWebsiteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (UpdateWebsiteTimer)
            {
                if (ticking)
                    return;
                ticking = true;
            }
            try
            {
                var client = new ApiClient();
                var list = new List<GameDetails>();
                var games = Games.ToArray();
                foreach (var g in games)
                {
                    var gd = new GameDetails()
                    {
                        AllowsSpectators = g.Spectator,
                        Id = g.Id,
                        GameId = g.GameGuid,
                        GameName = g.GameName,
                        Host = g.Username,
                        Name = g.Name,
                        InProgress = g.GameStatus == EHostedGame.GameInProgress,
                        PasswordProtected = g.HasPassword,
                        DateCreated = g.TimeStarted,
                        GameVersion = g.GameVersion,
						GameIconUrl = g.GameIconUrl,
						HostIconUrl = g.UserIconUrl,
                        IpAddress = g.IpAddress.ToString(),
                        Port = g.Port
                    };
                    list.Add(gd);
                }

                if (client.SetGameList(AppConfig.Instance.ApiKey, list) == false)
                {
                    Log.Warn("UpdateWebsiteTimerOnElapsed: Couldn't set the game list, some kinda error.");
                }
            }
            catch (Exception e)
            {
                Log.Error("UpdateWebsiteTimerOnElapsedError", e);
                throw;
            }
            finally
            {
                ticking = false;
            }
        }

        public void Dispose()
        {
            if(GameListener != null)
                GameListener.Dispose();
			UpdateWebsiteTimer.Dispose();
        }
    }
}