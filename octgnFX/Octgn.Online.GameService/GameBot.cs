/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;
using log4net;
using System.Threading;
using Octgn.Communication;
using Octgn.Communication.Chat;
using Octgn.Communication.Serializers;
using System.Threading.Tasks;
using Octgn.Library;

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
        private readonly Library.Communication.ClientAuthenticator _clientAuthenticator;

        private GameBot()
        {
            _chatClient = new Client(new TcpConnection(AppConfig.Instance.ServerPath), new JsonSerializer(), _clientAuthenticator = new Library.Communication.ClientAuthenticator());
            _chatClient.InitializeChat();
            _chatClient.RequestReceived += _chatClient_RequestReceived;
        }

        public async Task Start()
        {
            var client = new Octgn.Site.Api.ApiClient();
            var result = await client.CreateSession(AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword, AppConfig.Instance.DeviceId);

            _clientAuthenticator.SessionKey = result.SessionKey;
            _clientAuthenticator.UserId = result.UserId;
            _clientAuthenticator.DeviceId = AppConfig.Instance.DeviceId;

            await _chatClient.Connect();

            throw new NotImplementedException();
        }

        private void _chatClient_RequestReceived(object sender, RequestReceivedEventArgs args) {
            try {
                if (args.Request.Name == nameof(IClientCalls.HostGame)) {
                    var request = HostGameRequest.GetFromPacket(args.Request);

                    Log.InfoFormat("Host game from {0}", args.Request.Origin);
                    var endTime = DateTime.Now.AddSeconds(10);
                    while (SasUpdater.Instance.IsUpdating) {
                        Thread.Sleep(100);
                        if (endTime > DateTime.Now) throw new Exception("Couldn't host, sas is updating");
                    }
                    var id = GameManager.Instance.HostGame(request, new User(args.Request.Origin, true)).Result;
                    var game = GameManager.Instance.Games.FirstOrDefault(x => x.Id == id);

                    if (id == Guid.Empty) throw new InvalidOperationException("id == Guid.Empty");

                    var result = new Octgn.Communication.Chat.HostedGame {
                        GameGuid = game.GameGuid,
                        GameIconUrl = game.GameIconUrl,
                        GameName = game.GameName,
                        GameStatus = game.GameStatus.ToString(),
                        GameVersion = game.GameVersion,
                        HasPassword = game.HasPassword,
                        Id = game.Id,
                        IpAddress = game.IpAddress.ToString(),
                        Name = game.Name,
                        Port = game.Port,
                        Source = game.Source.ToString(),
                        Spectator = game.Spectator,
                        TimeStarted = game.TimeStarted,
                        UserIconUrl = game.UserIconUrl,
                        HostUserId = args.Request.Origin
                    };

                    args.Response = new Communication.Packets.ResponsePacket(args.Request, result);
                }
            } catch (Exception ex) {
                Log.Error($"{nameof(_chatClient_RequestReceived)}", ex);
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
