using System.Threading.Tasks;
using Octgn.Communication;
using System;
using Octgn.Communication.Packets;
using Octgn.Communication.Serializers;

namespace Octgn.Online.Hosting
{
    public class ServerHostingModule : IServerModule
    {
        private readonly Server _server;
        private readonly string _gameServerUserId;

        public ServerHostingModule(Server server, string gameServerUserId) {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _gameServerUserId = gameServerUserId ?? throw new ArgumentNullException(nameof(gameServerUserId));

            _requestHandler.Register(nameof(IClientHostingRPC.HostGame), OnHostGame);
            _requestHandler.Register(nameof(IClientHostingRPC.SignalGameStarted), OnSignalGameStarted);

            if(_server.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }
        }

        private async Task<ResponsePacket> OnSignalGameStarted(object sender, RequestReceivedEventArgs args) {
            var sendRequest = new RequestPacket(args.Request);
            var gsResp = await _server.Request(sendRequest, _gameServerUserId);

            return new ResponsePacket(args.Request, gsResp.Data);
        }

        private async Task<ResponsePacket> OnHostGame(object sender, RequestReceivedEventArgs args) {
            var sendRequest = new RequestPacket(args.Request);
            var gsResp = await _server.Request(sendRequest, _gameServerUserId);

            return new ResponsePacket(args.Request, gsResp.Data);
        }

        private readonly RequestHandler _requestHandler = new RequestHandler();

        public Task HandleRequest(object sender, RequestReceivedEventArgs args) {
            return _requestHandler.HandleRequest(sender, args);
        }

        public Task UserStatucChanged(object sender, UserStatusChangedEventArgs e) {
            return Task.CompletedTask;
        }
    }
}