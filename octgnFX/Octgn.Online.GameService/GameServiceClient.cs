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

namespace Octgn.Online.GameService
{
    public class GameServiceClient : IDisposable
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Client _chatClient;
        private readonly Octgn.Library.Communication.ClientAuthenticator _clientAuthenticator;

        public GameServiceClient() {
            _chatClient = new Client(new TcpConnection(AppConfig.Instance.ServerPath), new XmlSerializer(), _clientAuthenticator = new Octgn.Library.Communication.ClientAuthenticator());

            if (_chatClient.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }

            _chatClient.InitializeSubscriptionModule();
            _chatClient.RequestReceived += ChatClient_RequestReceived;
        }

        public async Task Start() {
            var client = new Octgn.Site.Api.ApiClient();
            var result = await client.CreateSession(AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword, AppConfig.Instance.DeviceId);

            _clientAuthenticator.SessionKey = result.SessionKey;
            _clientAuthenticator.UserId = result.UserId;
            _clientAuthenticator.DeviceId = AppConfig.Instance.DeviceId;

            await _chatClient.Connect();
        }

        private async Task ChatClient_RequestReceived(object sender, RequestReceivedEventArgs args) {
            if (args.Request.Name == nameof(IClientHostingRPC.HostGame)) {
                try {
                    var game = HostedGame.GetFromPacket(args.Request);
                    game.HostUser = args.Request.Origin;

                    Log.InfoFormat("Host game from {0}", args.Request.Origin);
                    var endTime = DateTime.Now.AddSeconds(10);
                    while (SasUpdater.Instance.IsUpdating) {
                        await Task.Delay(100);
                        if (endTime > DateTime.Now) throw new Exception("Couldn't host, sas is updating");
                    }
                    var id = await HostedGames.HostGame(game, args.Request.Origin);

                    if (id == Guid.Empty) throw new InvalidOperationException("id == Guid.Empty");

                    game = HostedGames.Get(id);

                    game.HostUser = args.Request.Origin;

                    args.Response = new Communication.Packets.ResponsePacket(args.Request, game);
                } catch (Exception ex) {
                    Log.Error($"{nameof(ChatClient_RequestReceived)}", ex);
                    args.Response = new Communication.Packets.ResponsePacket(args.Request, new ErrorResponseData(Communication.ErrorResponseCodes.UnhandledServerError, "Problem starting SAS", false));
                }

                args.IsHandled = true;
            }
        }

        public void Dispose() {
            Log.Info(nameof(GameServiceClient) + " Disposed");
        }
    }
}
