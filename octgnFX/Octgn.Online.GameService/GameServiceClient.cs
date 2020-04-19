/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Reflection;
using log4net;
using Octgn.Communication;
using Octgn.Communication.Modules.SubscriptionModule;
using Octgn.Communication.Serializers;
using System.Threading.Tasks;
using Octgn.Online.Hosting;
using System.Threading;
using Octgn.Communication.Modules;

namespace Octgn.Online.GameService
{
    public class GameServiceClient : Client, IDisposable
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DefaultHandshaker _handshaker;

        public GameServiceClient(IConnectionCreator connectionCreator) : base(connectionCreator, new XmlSerializer()) {
            if (connectionCreator == null) throw new ArgumentNullException(nameof(connectionCreator));

            if (!(connectionCreator.Handshaker is DefaultHandshaker defaultHandshaker))
                throw new InvalidOperationException($"{nameof(IConnectionCreator)} must use the {nameof(DefaultHandshaker)}. No other {nameof(IHandshaker)} is currently supported.");

            _handshaker = defaultHandshaker;

            if (Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }

            this.InitializeSubscriptionModule();
            this.InitializeStatsModule();

            RequestReceived += ChatClient_RequestReceived;
        }

        public async Task Start(CancellationToken cancellationToken = default(CancellationToken)) {
            Log.Info($"{nameof(Start)}: CreateSession");
            var client = new Octgn.Site.Api.ApiClient();
            var result = await client.CreateSession(AppConfig.Instance.ComUsername, AppConfig.Instance.ComPassword, AppConfig.Instance.ComDeviceId);
            if(result.Result.Type != Site.Api.LoginResultType.Ok) {
                throw new InvalidOperationException($"Couldn't not start. Error creating session: {result.Result.Type}");
            }

            _handshaker.SessionKey = result.SessionKey;
            _handshaker.UserId = result.UserId;
            _handshaker.DeviceId = AppConfig.Instance.ComDeviceId;

            Log.Info($"{nameof(Start)}: Connecting");

            await Connect(AppConfig.Instance.ComUrl, cancellationToken);
        }

        private async Task<object> ChatClient_RequestReceived(object sender, RequestReceivedEventArgs args) {
            if (args.Request.Name == nameof(IClientHostingRPC.HostGame)) {
                try {
                    var game = HostedGame.GetFromPacket(args.Request);
                    if (game == null) throw new InvalidOperationException("game is null");

                    game.HostUser = args.Request.Origin ?? throw new InvalidOperationException("args.Request.Origin is null");

                    Log.InfoFormat("Host game from {0}", args.Request.Origin);
                    var endTime = DateTime.Now.AddSeconds(10);
                    while (SasUpdater.Instance.IsUpdating) {
                        await Task.Delay(100);

                        if (endTime > DateTime.Now) throw new Exception("Couldn't host, sas is updating");
                    }

                    var id = await HostedGames.HostGame(game);

                    if (id == Guid.Empty) throw new InvalidOperationException("id == Guid.Empty");

                    game = HostedGames.Get(id);

                    if (game == null) throw new InvalidOperationException("game from HostedGames is null");
                    if (game.HostUser == null) throw new InvalidOperationException("game.HostUser is null");

                    return game;
                } catch (Exception ex) {
                    Log.Error($"{nameof(ChatClient_RequestReceived)}", ex);

                    return new ErrorResponseData(Communication.ErrorResponseCodes.UnhandledServerError, "Problem starting SAS", false);
                }
            }

            return null;
        }
    }
}
