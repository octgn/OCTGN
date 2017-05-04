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

        private readonly Client _chatClient;

        private GameBot()
        {
            _chatClient = new Client(new TcpConnection(AppConfig.Instance.ServerPath));
            _chatClient.DeliverableReceived += ChatClient_DeliverableReceived;
        }

        public void Start()
        {
            _chatClient.Connect(AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword);
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
                            args.Response = new Package(args.Deliverable.From, gameInfo);
                        }
                        return;
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
        }

        #endregion
    }
}
