/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Site.Api;
using System.Runtime.Caching;
using System.Threading;
using System.Timers;

using Timer = System.Timers.Timer;
using Octgn.Chat;
using System.Threading.Tasks;
using Octgn.Chat.Communication;

namespace Octgn.Online.GameService
{

    public class GameBot : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static GameBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static GameBot Instance {
            get {
                if (SingletonContext == null) {
                    lock (GameBotSingletonLocker) {
                        if (SingletonContext == null) {
                            SingletonContext = new GameBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private readonly MemoryCache _userRequests;
        private readonly Timer _refreshGamesTimer;
        private readonly Client _chatClient;

        private GameBot()
        {
            _userRequests = new MemoryCache(nameof(_userRequests));
            _refreshGamesTimer = new Timer(2000);
            _refreshGamesTimer.Start();
            _refreshGamesTimer.Elapsed += RefreshGamesTimerOnElapsed;
            //_xmpp = new XmppClientConnection(AppConfig.Instance.ServerPath);
            _chatClient = new Client(new TcpConnection(AppConfig.Instance.ServerPath));
            _chatClient.DeliverableReceived += ChatClient_DeliverableReceived;
        }

        private void RefreshGamesTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try {
                _refreshGamesTimer.Enabled = false;

                if (_userRequests.GetCount() == 0) return;
                var games = GameManager.Instance.Games.ToArray();

                foreach (var game in games) {
                    var strname = "hostrequest_" + game.Id;
                    if (_userRequests.Contains(strname)) {
                        _userRequests.Remove(strname);

                        SendGameReady(game);
                    }
                }

            } catch (Exception e) {
                Log.Error("[RefreshGamesTimerOnElapsed]", e);
            } finally {
                _refreshGamesTimer.Enabled = true;
            }
        }

        public void Start()
        {
            _chatClient.Connect(AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword);
        }

        public async Task SendGameReady(Skylabs.Lobby.HostedGameData game)
        {
            await _chatClient.Request(new Package(game.Username, game));
        }

        private void ChatClient_DeliverableReceived(object sender, DeliverableReceivedEventArgs args)
        {
            try {
                if (args.Deliverable is Package) {
                    var package = args.Deliverable as Package;

                    if (package.Contents is HostGameRequest) {
                        var req = package.Contents as HostGameRequest;

                        Log.InfoFormat("Host game from {0}", args.Deliverable.From);
                        var endTime = DateTime.Now.AddSeconds(10);
                        while (SasUpdater.Instance.IsUpdating) {
                            Thread.Sleep(100);
                            if (endTime > DateTime.Now) throw new Exception("Couldn't host, sas is updating");
                        }
                        var id = GameManager.Instance.HostGame(req, new Skylabs.Lobby.User(args.Deliverable.From)).Result;
                        var game = GameManager.Instance.Games.FirstOrDefault(x => x.Id == id);

                        HostedGameInfo gameInfo = new HostedGameInfo {
                            GameGuid = game.GameGuid,
                            GameIconUrl = game.GameIconUrl,
                            GameName = game.GameName,
                            GameStatus = game.GameStatus.ToString(),
                            GameVersion = game.GameVersion,
                            HasPassword = game.HasPassword,
                            Id =game.Id,
                            IpAddress = game.IpAddress.ToString(),
                            Name = game.Name,
                            Port = game.Port,
                            Source = game.Source.ToString(),
                            Spectator = game.Spectator,
                            TimeStarted = game.TimeStarted,
                            UserIconUrl = game.UserIconUrl,
                            Username = game.Username
                        };

                        if (id == Guid.Empty) throw new InvalidOperationException("id == Guid.Empty");

                        if (id != Guid.Empty) {
                            _userRequests.Add("hostrequest_" + id, id, DateTimeOffset.UtcNow.AddSeconds(30));
                            args.Response = new Package(args.Deliverable.From, gameInfo);
                        }
                        return;
                    } else if (package.Contents is string) {
                        var contents = package.Contents as string;

                        if (contents == "gamelist") {
                            // If someone tried to refresh their game list too soon, f them
                            if (_userRequests.Contains("refreshrequest_" + args.Deliverable.From.ToLower()))
                                return;
                            // Mark the user as already requested a list for the next 15 seconds
                            _userRequests.Add("refreshrequest_" + args.Deliverable.From.ToLower(), 1, DateTimeOffset.UtcNow.AddSeconds(15));
                            var list = GameManager.Instance.Games;

                            _chatClient.Request(new Package(args.Deliverable.From, list));
                        }
                    }
                }
                Log.Warn($"Deliverable not handled. {args.Deliverable.ToString()}");
            } catch (Exception ex) {
                Log.Error("[Bot]XmppOnOnMessage Error", ex);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Log.Info(nameof(GameBot) + " Disposed");
            _userRequests.Dispose();
            _refreshGamesTimer.Elapsed -= RefreshGamesTimerOnElapsed;
        }

        #endregion
    }
}
